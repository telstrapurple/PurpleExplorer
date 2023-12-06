using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using DynamicData;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.Views;
using ReactiveUI;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using Microsoft.Azure.ServiceBus;
using PurpleExplorer.Services;
using Message = PurpleExplorer.Models.Message;

namespace PurpleExplorer.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ITopicHelper _topicHelper;
    private readonly IQueueHelper _queueHelper;
    private readonly ILoggingService _loggingService;
    private string _messageTabHeader;
    private string _dlqTabHeader;
    private string _topicTabHeader;
    private string _queueTabHeader;
        
    private ServiceBusSubscription _currentSubscription;
    private ServiceBusTopic _currentTopic;
    private ServiceBusQueue _currentQueue;
    private Message _currentMessage;
    private IObservable<bool> _queueLevelActionEnabled;
    private IAppState _appState;
    private readonly IApplicationService _applicationService;
    private readonly IModalWindowService _modalWindowService;

    public ObservableCollection<Message> Messages { get; }
    public ObservableCollection<Message> DlqMessages { get; }
    public ObservableCollection<ServiceBusResource> ConnectedServiceBuses { get; }
        
    public ServiceBusConnectionString ConnectionString { get; set; }

    public string MessagesTabHeader
    {
        get => _messageTabHeader;
        set => this.RaiseAndSetIfChanged(ref _messageTabHeader, value);
    }

    public string DlqTabHeader
    {
        get => _dlqTabHeader;
        set => this.RaiseAndSetIfChanged(ref _dlqTabHeader, value);
    }

    public string TopicTabHeader
    {
        get => _topicTabHeader;
        set => this.RaiseAndSetIfChanged(ref _topicTabHeader, value);
    }
        
    public string QueueTabHeader
    {
        get => _queueTabHeader;
        set => this.RaiseAndSetIfChanged(ref _queueTabHeader, value);
    }
        
    public ServiceBusSubscription CurrentSubscription
    {
        get => _currentSubscription;
        set => this.RaiseAndSetIfChanged(ref _currentSubscription, value);
    }

    public ServiceBusTopic CurrentTopic
    {
        get => _currentTopic;
        set => this.RaiseAndSetIfChanged(ref _currentTopic, value);
    }

    public Message CurrentMessage
    {
        get => _currentMessage;
        set => this.RaiseAndSetIfChanged(ref _currentMessage, value);
    }

    public ServiceBusQueue CurrentQueue
    {
        get => _currentQueue;
        set => this.RaiseAndSetIfChanged(ref _currentQueue, value);
    }
        
    public MessageCollection CurrentMessageCollection
    {
        get
        {
            if (CurrentSubscription != null)
                return CurrentSubscription;
            if (CurrentQueue != null)
                return CurrentQueue;
            return null;
        }
    }

    public ILoggingService LoggingService => _loggingService;

    public Version AppVersion => Assembly.GetExecutingAssembly().GetName().Version;
    public string AppVersionText { get; set; }

    public IObservable<bool> QueueLevelActionEnabled
    {
        get => _queueLevelActionEnabled;
        set => this.RaiseAndSetIfChanged(ref _queueLevelActionEnabled, value);
    }

    public MainWindowViewModel(ILoggingService loggingService, ITopicHelper topicHelper, IQueueHelper queueHelper, IAppState appState, IApplicationService applicationService, IModalWindowService modalWindowService)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _topicHelper = topicHelper ?? throw new ArgumentNullException(nameof(topicHelper));
        _queueHelper = queueHelper ?? throw new ArgumentNullException(nameof(queueHelper));
        _appState = appState ?? throw new ArgumentNullException(nameof(appState));
        _applicationService = applicationService;
        _modalWindowService = modalWindowService;

        Messages = new ObservableCollection<Message>();
        DlqMessages = new ObservableCollection<Message>();
        ConnectedServiceBuses = new ObservableCollection<ServiceBusResource>();

        _queueLevelActionEnabled = this.WhenAnyValue(
            x => x.CurrentSubscription,
            x=> x.CurrentQueue,
            (subscription, queue) => subscription != null || queue != null 
        );

        RefreshTabHeaders();

        AppVersionText = AppVersion.ToString(3);
        LoggingService.Log($"PurpleExplorer v{AppVersionText}");

        // Checking for new version asynchronous. no need to await on it
#pragma warning disable 4014
        CheckForNewVersion();
#pragma warning restore 4014
    }

    public async void ConnectionBtnPopupCommand()
    {
        ShowConnectionStringWindow();
    }

    internal async void ShowConnectionStringWindow()
    {
        var viewModel = new ConnectionStringWindowViewModel(_appState);

        var returnedViewModel =
            await _modalWindowService.ShowModalWindow
                <ConnectionStringWindow, ConnectionStringWindowViewModel>(
                    viewModel);

        if (returnedViewModel.Cancel)
        {
            return;
        }

        ConnectionString = new ServiceBusConnectionString
        {
            ConnectionString = returnedViewModel.ConnectionString,
            UseManagedIdentity= returnedViewModel.UseManagedIdentity
        };

        if (string.IsNullOrEmpty(ConnectionString.ConnectionString))
        {
            return;
        }

        try
        {
            LoggingService.Log("Connecting...");

            var namespaceInfo = await _topicHelper.GetNamespaceInfo(ConnectionString);
            var topics = await _topicHelper.GetTopicsAndSubscriptions(ConnectionString);
            var queues = await _queueHelper.GetQueues(ConnectionString);

            var serviceBusResource = new ServiceBusResource
            {
                Name = namespaceInfo.Name,
                CreatedTime = namespaceInfo.CreatedTime,
                ConnectionString = ConnectionString
            };

            serviceBusResource.AddQueues(queues.ToArray());
            serviceBusResource.AddTopics(topics.ToArray());
            ConnectedServiceBuses.Add(serviceBusResource);
            LoggingService.Log("Connected to Service Bus: " + namespaceInfo.Name);
        }
        catch (ArgumentException)
        {
            await MessageBoxHelper.ShowError("The connection string is invalid.");
            LoggingService.Log("Connection failed: The connection string is invalid");
        }
        catch (UnauthorizedException)
        {
            await MessageBoxHelper.ShowError("Unable to connect to Service Bus; unauthorized.");
            LoggingService.Log("Connection failed: Unauthorized");
        }
        catch (ServiceBusException ex)
        {
            await MessageBoxHelper.ShowError($"Unable to connect to Service Bus; {ex.Message}");
            LoggingService.Log($"Connection failed: {ex.Message}");
        }
        finally
        {
            RefreshTabHeaders();
        }
    }

    public void RefreshTabHeaders()
    {
        if (CurrentMessageCollection is null)
        {
            MessagesTabHeader = "Messages";
            DlqTabHeader = "Dead-letter";
        }
        else
        {
            MessagesTabHeader = $"Messages ({CurrentMessageCollection.MessageCount})";
            DlqTabHeader = $"Dead-letter ({CurrentMessageCollection.DlqCount})";
        }

        var topicCount = ConnectedServiceBuses.Sum(x => x.Topics.Count);
        var queueCount = ConnectedServiceBuses.Sum(x => x.Queues.Count);
        if (topicCount > 0 || queueCount > 0)
        {
            TopicTabHeader = $"Topics ({topicCount})";
            QueueTabHeader = $"Queues ({queueCount})";
        }
        else
        {
            TopicTabHeader = "Topics";
            QueueTabHeader = "Queues";
        }
    }

    public async Task RefreshConnectedServiceBuses()
    {
        foreach (var serviceBusResource in ConnectedServiceBuses)
        {
            var topicsAndSubscriptions = await _topicHelper.GetTopicsAndSubscriptions(serviceBusResource.ConnectionString);
            var serviceBusQueues = await _queueHelper.GetQueues(serviceBusResource.ConnectionString);

            serviceBusResource.Topics.Clear();
            serviceBusResource.Queues.Clear();
            serviceBusResource.AddTopics(topicsAndSubscriptions.ToArray());
            serviceBusResource.AddQueues(serviceBusQueues.ToArray());
        }
    }
        
    public async void AddMessage()
    {
        var viewModal = new AddMessageWindowViewModal();

        var returnedViewModal =
            await _modalWindowService.ShowModalWindow<AddMessageWindow, AddMessageWindowViewModal>(viewModal);

        if (returnedViewModal.Cancel)
        {
            return;
        }

        var messageText = returnedViewModal.Message.Trim();
        if (string.IsNullOrEmpty(messageText))
        {
            return;
        }
            
        LoggingService.Log("Sending message...");
        if (CurrentTopic != null)
        {
            var connectionString = CurrentTopic.ServiceBus.ConnectionString;
            await _topicHelper.SendMessage(connectionString, CurrentTopic.Name, messageText);
        }

        if (CurrentQueue != null)
        {
            var connectionString = CurrentQueue.ServiceBus.ConnectionString;
            await _queueHelper.SendMessage(connectionString, CurrentQueue.Name, messageText);
        }
        LoggingService.Log("Message sent");
    }

    public async void TransferDeadletterMessages()
    {
        string dlqPath = null;
        string transferTo = null; 
        if (_currentSubscription != null)
        {
            transferTo = $"{_currentTopic.Name}/{_currentSubscription.Name}";
            dlqPath = $"{transferTo}/$DeadLetterQueue";
        }

        if (_currentQueue != null)
        {
            transferTo = $"{_currentQueue.Name}";
            dlqPath = $"{transferTo}/$DeadLetterQueue";
        }
            
        var buttonResult = await MessageBoxHelper.ShowConfirmation(
            "Transferring messages from DLQ",
            $"Are you sure you would like to transfer ALL the messages on {dlqPath} back to {transferTo}?");
            
        // Because buttonResult can be None or No
        if (buttonResult != ButtonResult.Yes)
        {
            CurrentMessage = null;
            return;
        }

        LoggingService.Log($"Transferring ALL messages in {dlqPath}... (might take some time)");
        long transferCount = -1;
        if (CurrentSubscription != null)
        {
            var connectionString = CurrentSubscription.Topic.ServiceBus.ConnectionString;
            transferCount = await _topicHelper.TransferDlqMessages(connectionString, _currentTopic.Name,
                _currentSubscription.Name);
        }

        if (CurrentQueue != null)
        {
            var connectionString = CurrentQueue.ServiceBus.ConnectionString;
            transferCount = await _queueHelper.TransferDlqMessages(connectionString, _currentQueue.Name);
        }
        LoggingService.Log($"Transferred {transferCount} messages in {dlqPath}");
    }

    public async void PurgeMessages(string isDlqText)
    {
        var isDlq = Convert.ToBoolean(isDlqText);
        string purgingPath = null;
        if (_currentSubscription != null)
        {
            purgingPath = isDlq
                ? $"{_currentTopic.Name}/{_currentSubscription.Name}/$DeadLetterQueue"
                : $"{_currentTopic.Name}/{_currentSubscription.Name}";
        }

        if (_currentQueue != null)
        {
            purgingPath = isDlq
                ? $"{_currentQueue.Name}/$DeadLetterQueue"
                : $"{_currentQueue.Name}";
        }

        var buttonResult = await MessageBoxHelper.ShowConfirmation(
            $"Purging messages from {purgingPath}",
            $"Are you sure you would like to purge ALL the messages from {purgingPath}?");

        // Because buttonResult can be None or No
        if (buttonResult != ButtonResult.Yes)
        {
            CurrentMessage = null;
            return;
        }

        LoggingService.Log($"Purging ALL messages in {purgingPath}... (might take some time)");
        long purgedCount = -1;
        if (CurrentSubscription != null)
        {
            var connectionString = CurrentSubscription.Topic.ServiceBus.ConnectionString;
            purgedCount = await _topicHelper.PurgeMessages(connectionString, _currentTopic.Name,
                _currentSubscription.Name, isDlq);

            if (!isDlq)
                CurrentSubscription.ClearMessages();
            else
                CurrentSubscription.ClearDlqMessages();
        }

        if (CurrentQueue != null)
        {
            var connectionString = CurrentQueue.ServiceBus.ConnectionString;
            purgedCount = await _queueHelper.PurgeMessages(connectionString, _currentQueue.Name, isDlq);
                
            if (!isDlq)
                CurrentQueue.ClearMessages();
            else
                CurrentQueue.ClearDlqMessages();
        }
        LoggingService.Log($"Purged {purgedCount} messages in {purgingPath}");

        // Refreshing messages
        await FetchMessages();
    }

    public async Task Refresh()
    {
        await RefreshConnectedServiceBuses();
        RefreshTabHeaders();
        await FetchMessages();
    }
        
    public async Task FetchMessages()
    {
        LoggingService.Log("Fetching messages...");

        await Task.WhenAll(
            FetchSubscriptionMessages(),
            FetchSubscriptionDlqMessages(),
            FetchQueueMessages(),
            FetchQueueDlqMessages()
        );

        LoggingService.Log("Fetched messages");
    }

    public void SetSelectedSubscription(ServiceBusSubscription subscription)
    {
        CurrentSubscription = subscription;
        CurrentTopic = subscription.Topic;
        LoggingService.Log("Subscription selected: " + subscription.Name);
    }

    public void SetSelectedTopic(ServiceBusTopic selectedTopic)
    {
        CurrentTopic = selectedTopic;
        LoggingService.Log("Topic selected: " + selectedTopic.Name);
    }

    public void SetSelectedMessage(Message message)
    {
        CurrentMessage = message;
        LoggingService.Log("Message selected: " + message.MessageId);
    }

    public void SetSelectedQueue(ServiceBusQueue selectedQueue)
    {
        CurrentQueue = selectedQueue;
        LoggingService.Log("Queue selected: " + selectedQueue.Name);
    }

    public async Task ShowSettings()
    {
        await _modalWindowService.ShowModalWindow<AppSettingsWindow>(_appState as AppState);
    }
        
    public void ClearAllSelections()
    {
        CurrentSubscription = null;
        CurrentTopic = null;
        CurrentQueue = null;
        CurrentMessage = null;
        Messages.Clear();
        DlqMessages.Clear();
        RefreshTabHeaders();
    }
    
    private async Task CheckForNewVersion()
    {
        var latestRelease = await AppVersionHelper.GetLatestRelease();
        var latestReleaseVersion = new Version(latestRelease.Name);
        if (latestReleaseVersion > AppVersion)
        {
            AppVersionText = $"new v{latestReleaseVersion} is available";
            this.RaisePropertyChanged(nameof(AppVersionText));

            var message =
                $"New version v{latestReleaseVersion} is available. \n Download today at {latestRelease.HtmlUrl}";
            LoggingService.Log(message);
            await MessageBoxHelper.ShowMessage("New version available", message);
        }
        else
        {
            LoggingService.Log($"v{AppVersion} is the latest released version");
        }
    }

    private async Task FetchSubscriptionMessages()
    {
        if (CurrentSubscription == null)
        {
            return;
        }
            
        Messages.Clear();
        CurrentSubscription.ClearMessages();
        var messages =
            await _topicHelper.GetMessagesBySubscription(CurrentSubscription.Topic.ServiceBus.ConnectionString,
                CurrentSubscription.Topic.Name,
                CurrentSubscription.Name);
        CurrentSubscription.AddMessages(messages);
        Messages.AddRange(messages);
    }

    private async Task FetchSubscriptionDlqMessages()
    {
        if (CurrentSubscription == null)
        {
            return;
        }
            
        DlqMessages.Clear();
        CurrentSubscription.ClearDlqMessages();
        var dlqMessages =
            await _topicHelper.GetDlqMessages(CurrentSubscription.Topic.ServiceBus.ConnectionString,
                CurrentSubscription.Topic.Name, CurrentSubscription.Name);
        CurrentSubscription.AddDlqMessages(dlqMessages);
        DlqMessages.AddRange(dlqMessages);
    }
        
    private async Task FetchQueueMessages()
    {
        if (CurrentQueue == null)
        {
            return;
        }
            
        Messages.Clear();
        CurrentQueue.ClearMessages();
        var messages = await _queueHelper.GetMessages(CurrentQueue.ServiceBus.ConnectionString, CurrentQueue.Name);
        CurrentQueue.AddMessages(messages);
        Messages.AddRange(messages);
    }
        
    private async Task FetchQueueDlqMessages()
    {
        if (CurrentQueue == null)
        {
            return;
        }
            
        DlqMessages.Clear();
        CurrentQueue.ClearDlqMessages();
        var messages = await _queueHelper.GetDlqMessages(CurrentQueue.ServiceBus.ConnectionString, CurrentQueue.Name);
        CurrentQueue.AddDlqMessages(messages);
        DlqMessages.AddRange(messages);
    }
}