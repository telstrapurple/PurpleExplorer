using System.Collections.ObjectModel;
using FluentAssertions;
using Microsoft.Azure.ServiceBus.Management;
using Moq;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.Services;
using PurpleExplorer.ViewModels;

namespace PurpleExplorerTest.ViewModels;

public class MainWindowViewModelTest
{
    [Fact]
    public void Constructor_sets_up_public_properties()
    {
        //  Act.
        var sut = CreateSut(out var loggingService, out _, out _, out _, out _);
        
        //  Assert.
        sut.AppVersion.Major.Should().Be(1);
        sut.AppVersion.Minor.Should().Be(0);
        sut.AppVersion.MinorRevision.Should().Be(0);
        sut.AppVersionText.Should().Be("1.0.0");
        
        sut.MessagesTabHeader.Should().Be("Messages");
        sut.DlqTabHeader.Should().Be("Dead-letter");
        sut.TopicTabHeader.Should().Be("Topics");
        sut.QueueTabHeader.Should().Be("Queues");

        sut.CurrentSubscription.Should().BeNull();
        sut.CurrentTopic.Should().BeNull();
        sut.CurrentMessage.Should().BeNull();
        sut.CurrentQueue.Should().BeNull();
        sut.CurrentMessageCollection.Should().BeNull();

        sut.LoggingService.Should().Be(loggingService.Object);

        sut.QueueLevelActionEnabled.Should().NotBeNull();

        sut.ConnectionString.Should().BeNull();

        sut.Messages.Count().Should().Be(0);
        sut.DlqMessages.Count().Should().Be(0);
        sut.ConnectedServiceBuses.Count().Should().Be(0);
    }

    #region ConnectionBtnPopCommand

    [Fact]
    public void ConnectionBtnPopupCommand_does_not_change_connectionstring_When_user_cancels()
    {
        var myOriginalConnectionString = new ServiceBusConnectionString();

        var sut = CreateSut(out _, out var appState, out var modalWindowService, out var topicHelper, out var queueHelper)
            .With(s => s.ConnectionString = myOriginalConnectionString);

        appState.Setup(p => p.SavedConnectionStrings)
            .Returns(new ObservableCollection<ServiceBusConnectionString>());

        modalWindowService.Setup_ShowModalWindow(
            new ConnectionStringWindowViewModel(appState.Object)
                .With(c=> c.Cancel = true)
        );
        
        //  Act.
        sut.ConnectionBtnPopupCommand();
        
        //  Assert.
        sut.ConnectionString.Should().Be(myOriginalConnectionString);
        // Verify no call is made to any topic or queue to verify nothing was done
        // when the user chose Cancel. A bit crude, but I found no better way.
        topicHelper.VerifyAll();
        queueHelper.VerifyAll();
    }

    [Fact]
    public void ConnectionBtnPopupCommand_sets_connectionstring_and_nothing_else_When_ok_but_no_connectionstring_value()
    {
        var myOriginalConnectionString = new ServiceBusConnectionString();
        var myNewConnectionString = new ServiceBusConnectionString
        {
            Name = string.Empty,
            ConnectionString = string.Empty,
            UseManagedIdentity = true,
        };

        var sut = CreateSut(out _, out var appState, out var modalWindowService, out var topicHelper, out var queueHelper)
            .With(s => s.ConnectionString = myOriginalConnectionString);

        appState.Setup(p => p.SavedConnectionStrings)
            .Returns(new ObservableCollection<ServiceBusConnectionString>());

        modalWindowService.Setup_ShowModalWindow(
            new ConnectionStringWindowViewModel(appState.Object)
                .With(c=> c.Cancel = false)
                .With(c => c.ConnectionString = myNewConnectionString.ConnectionString)
                .With(c => c.UseManagedIdentity = myNewConnectionString.UseManagedIdentity)
        );
        
        //  Act
        sut.ConnectionBtnPopupCommand();
        
        //  Assert.
        sut.ConnectionString.Should().NotBe(myOriginalConnectionString);
        Assert.True(AreSameExceptName(myNewConnectionString, sut.ConnectionString));

        // Verify no call is made to any topic or queue to verify nothing was done
        // when the user chose Cancel. A bit crude, but I found no better way.
        topicHelper.VerifyAll();
        queueHelper.VerifyAll();
    }

    [Fact]
    public void ConnectionBtnPopupCommand_sets_ConnectedServiceBuses_When_ok_with_connectionstring_value()
    {
        var myOriginalConnectionString = new ServiceBusConnectionString();
        var myNewConnectionString = new ServiceBusConnectionString
        {
            Name = string.Empty,
            ConnectionString = "MyNewConnectionString",
            UseManagedIdentity = true,
        };
        var myNameSpaceInfo = new NamespaceInfo
        {
            Name = "MyNameSpaceInfoName",
            CreatedTime = new DateTime(1906, 12, 09),
        };

        var sut = CreateSut(out _, out var appState, out var modalWindowService, out var topicHelper, out var queueHelper)
            .With(s => s.ConnectionString = myOriginalConnectionString);

        appState.Setup(p => p.SavedConnectionStrings)
            .Returns(new ObservableCollection<ServiceBusConnectionString>());

        modalWindowService.Setup_ShowModalWindow(
            new ConnectionStringWindowViewModel(appState.Object)
                .With(c=> c.Cancel = false)
                .With(c => c.ConnectionString = myNewConnectionString.ConnectionString)
                .With(c => c.UseManagedIdentity = myNewConnectionString.UseManagedIdentity)
        );

        topicHelper.Setup(m => m.GetNamespaceInfo(It_Is_SameExceptName(myNewConnectionString)))
            .ReturnsAsync(myNameSpaceInfo);

        topicHelper.Setup(m => m.GetTopicsAndSubscriptions(It_Is_SameExceptName(myNewConnectionString)))
            .ReturnsAsync(new List<ServiceBusTopic>());

        queueHelper.Setup(m => m.GetQueues(It_Is_SameExceptName(myNewConnectionString)))
            .ReturnsAsync(new List<ServiceBusQueue>());
        
        //  Act
        sut.ConnectionBtnPopupCommand();
        
        //  Assert.
        sut.ConnectionString.Should().NotBe(myOriginalConnectionString);
        Assert.True(AreSameExceptName(myNewConnectionString, sut.ConnectionString));

        sut.ConnectedServiceBuses.Single().Name.Should().Be(myNameSpaceInfo.Name);
        sut.ConnectedServiceBuses.Single().CreatedTime.Should().Be(myNameSpaceInfo.CreatedTime);
        
        AreSameExceptName(myNewConnectionString, sut.ConnectedServiceBuses.Single().ConnectionString);
        
        // Without rewriting a lot we cannot test `ServiceBusQueue` nor `ServiceBusTopic` 
        // as they cannot be constructed outside Microsoft's own framework due to
        // constructor visibility.
        // So we just check there is something. Something empty.
        sut.ConnectedServiceBuses.Single().Queues.Should().BeEmpty();
        sut.ConnectedServiceBuses.Single().Topics.Should().BeEmpty();
    }

    #endregion

    #region RefreshTabHeaders tests.

    [Fact]
    public void RefreshTabHeaders_sets_tab_headers_When_no_currentmessage_and_no_topics_nor_queues()
    {
        var sut = CreateSut(out _, out _, out _, out _, out _);
        
        //  Act.
        sut.RefreshTabHeaders();
        
        //  Assert.
        sut.MessagesTabHeader.Should().Be("Messages");
        sut.DlqTabHeader.Should().Be("Dead-letter");
        sut.TopicTabHeader.Should().Be("Topics");
        sut.QueueTabHeader.Should().Be("Queues");
    }

    [Fact]
    public void RefreshTabHeaders_sets_tab_headers_When_no_currentmessage_but_topics_and_queues()
    {
        var sut = CreateSut(out _, out _, out _, out _, out _);
        sut.ConnectedServiceBuses.Add(new ServiceBusResource()
            // Add a topic. We test it is set later.
            .With(sb => sb.AddTopics( new ServiceBusTopic()))
            // Due to innards of `ServiceBugQueue` we cannot create them
            // and have to create en empty list - which might, or might not, 
            // happen in real life.
            .With(sb => sb.AddQueues( Array.Empty<ServiceBusQueue>()))
        );

        //  Act.
        sut.RefreshTabHeaders();
        
        //  Assert.
        sut.MessagesTabHeader.Should().Be("Messages");
        sut.DlqTabHeader.Should().Be("Dead-letter");
        sut.TopicTabHeader.Should().Be("Topics (1)");
        sut.QueueTabHeader.Should().Be("Queues (0)");
    }

    #endregion

    /// <summary>Returns true if the expected and actual contains the same values.*
    /// False otherwise.
    /// *)Note that Name is not compared as this is an implementation for a special case.
    /// </summary>
    /// <returns></returns>
    static bool AreSameExceptName(
        ServiceBusConnectionString expected,
        ServiceBusConnectionString actual)
    {
        return expected.ConnectionString == actual.ConnectionString &&
               expected.UseManagedIdentity == actual.UseManagedIdentity;
    } 

    private static MainWindowViewModel CreateSut(
        out Mock<ILoggingService> loggingServiceMock,
        out Mock<IAppState> appStateMock, 
        out Mock<IModalWindowService> modalWindowServiceMock,
        out Mock<ITopicHelper> topicHelperMock,
        out Mock<IQueueHelper> queueHelperMock
    )
    {
        var loggingService = new Mock<ILoggingService>();
        var topicHelper = new Mock<ITopicHelper>(MockBehavior.Strict);
        var queueHelper = new Mock<IQueueHelper>(MockBehavior.Strict);
        var appState = new Mock<IAppState>(MockBehavior.Strict);
        var applicationService = new Mock<IApplicationService>(MockBehavior.Strict);
        var modalWindowService = new Mock<IModalWindowService>(MockBehavior.Strict);
        
        var sut = new MainWindowViewModel(loggingService.Object, topicHelper.Object, queueHelper.Object,
            appState.Object, applicationService.Object, modalWindowService.Object);

        loggingServiceMock = loggingService;
        appStateMock = appState;
        modalWindowServiceMock = modalWindowService;
        topicHelperMock = topicHelper;
        queueHelperMock = queueHelper;
        return sut;
    }

    private static ServiceBusConnectionString It_Is_SameExceptName(ServiceBusConnectionString myNewConnectionString)
    {
        // // We do not compare name as for this case it is not interesting.
        return It.Is<ServiceBusConnectionString>(x => 
            AreSameExceptName(x, myNewConnectionString)
        );
    }
}