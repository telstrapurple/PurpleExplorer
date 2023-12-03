using FluentAssertions;
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

    #endregion

    private static MainWindowViewModel CreateSut(
        out Mock<ILoggingService> loggingServiceMock,
        out Mock<IAppState> appStateMock, 
        out Mock<IModalWindowService> modalWindowServiceMock,
        out Mock<ITopicHelper> topicHelperMock,
        out Mock<IQueueHelper> queueHelperMock
    )
    {
        var loggingService = new Mock<ILoggingService>();
        var topicHelper = new Mock<ITopicHelper>();
        var queueHelper = new Mock<IQueueHelper>();
        var appState = new Mock<IAppState>();
        var applicationService = new Mock<IApplicationService>();
        var modalWindowService = new Mock<IModalWindowService>();
        
        var sut = new MainWindowViewModel(loggingService.Object, topicHelper.Object, queueHelper.Object,
            appState.Object, applicationService.Object, modalWindowService.Object);

        loggingServiceMock = loggingService;
        appStateMock = appState;
        modalWindowServiceMock = modalWindowService;
        topicHelperMock = topicHelper;
        queueHelperMock = queueHelper;
        return sut;
    }
}