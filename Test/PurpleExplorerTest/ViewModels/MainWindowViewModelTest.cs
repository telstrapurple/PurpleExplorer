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
        var sut = CreateSut(out var loggingService);
        
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

    private static MainWindowViewModel CreateSut(
        out Mock<ILoggingService> loggingServiceMock
    )
    {
        var loggingService = new Mock<ILoggingService>();
        var topicHelper = new Mock<ITopicHelper>();
        var queueHelper = new Mock<IQueueHelper>();
        var appState = new Mock<IAppState>();

        var sut = new MainWindowViewModel(loggingService.Object, topicHelper.Object, queueHelper.Object,
            appState.Object);

        loggingServiceMock = loggingService;
        return sut;
    }
}