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
using MessageBox.Avalonia.Enums;
using Microsoft.Azure.ServiceBus;
using PurpleExplorer.Services;
using Splat;
using Message = PurpleExplorer.Models.Message;

namespace PurpleExplorer.ViewModels
{
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
        private string _connectionString;
        private IObservable<bool> _queueLevelActionEnabled;
        private MessageCollection _currentMessageCollection;
        
        public ObservableCollection<Message> Messages { get; }
        public ObservableCollection<Message> DlqMessages { get; }
        public ObservableCollection<ServiceBusResource> ConnectedServiceBuses { get; }
        
        public string ConnectionString
        {
            get => _connectionString;
            set => this.RaiseAndSetIfChanged(ref _connectionString, value);
        }

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
        public MainWindowViewModel()
        {
            _loggingService = Locator.Current.GetService<ILoggingService>();
            _topicHelper = Locator.Current.GetService<ITopicHelper>();
            _queueHelper = Locator.Current.GetService<IQueueHelper>();

            Messages = new ObservableCollection<Message>();
            DlqMessages = new ObservableCollection<Message>();
            ConnectedServiceBuses = new ObservableCollection<ServiceBusResource>();

            _queueLevelActionEnabled = this.WhenAnyValue(
                x => x.CurrentSubscription,
                x=> x.CurrentQueue,
                (subscription, queue) => subscription != null || queue != null 
            );

            RefreshTabHeaders();

            AppVersionText = AppVersion.ToString();
            // Checking for new version asynchronous. no need to await on it
#pragma warning disable 4014
            CheckForNewVersion();
#pragma warning restore 4014
        }

        private async Task CheckForNewVersion()
        {
            var latestRelease = await AppVersionHelper.GetLatestRelease();
            var latestReleaseVersion = new Version(latestRelease.name);
            if (latestReleaseVersion > AppVersion)
            {
                AppVersionText = $"new v{latestReleaseVersion} is available";
                this.RaisePropertyChanged(nameof(AppVersionText));

                var message =
                    $"New version v{latestReleaseVersion} is available. \n Download today at {latestRelease.html_url}";
                LoggingService.Log(message);
                await MessageBoxHelper.ShowMessage("New version available", message);
            }
            else
            {
                LoggingService.Log($"v{AppVersion} is the latest released version");
            }
        }

        public async void ConnectionBtnPopupCommand()
        {
            var viewModel = new ConnectionStringWindowViewModel();

            var returnedViewModel =
                await ModalWindowHelper.ShowModalWindow<ConnectionStringWindow, ConnectionStringWindowViewModel>(
                    viewModel);

            if (returnedViewModel.Cancel)
            {
                return;
            }

            ConnectionString = returnedViewModel.ConnectionString?.Trim();

            if (string.IsNullOrEmpty(ConnectionString))
            {
                return;
            }

            try
            {
                LoggingService.Log("Connecting...");

                var namespaceInfo = await _topicHelper.GetNamespaceInfo(ConnectionString);
                var topics = await _topicHelper.GetTopics(ConnectionString);
                var queues = await _queueHelper.GetQueues(ConnectionString);

                var newResource = new ServiceBusResource
                {
                    Name = namespaceInfo.Name,
                    ConnectionString = this.ConnectionString
                };

                newResource.AddQueues(queues.ToArray());
                newResource.AddTopics(topics.ToArray());
                ConnectedServiceBuses.Add(newResource);
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

        public async Task FetchSubscriptionMessages()
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

        public async Task FetchSubscriptionDlqMessages()
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
        
        public async Task FetchQueueMessages()
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
        
        public async Task FetchQueueDlqMessages()
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
        
        public void RefreshTabHeaders()
        {
            if (CurrentMessageCollection != null)
            {
                MessagesTabHeader = $"Messages ({CurrentMessageCollection.MessageCount})";
                DlqTabHeader = $"Dead-letter ({CurrentMessageCollection.DlqCount})";
            }
            else
            {
                MessagesTabHeader = "Messages";
                DlqTabHeader = "Dead-letter";
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

        public async void AddMessage()
        {
            var viewModal = new AddMessageWindowViewModal();

            var returnedViewModal =
                await ModalWindowHelper.ShowModalWindow<AddMessageWindow, AddMessageWindowViewModal>(viewModal);

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
            if (_currentSubscription != null)
            {
                dlqPath = $"{_currentTopic.Name}/{_currentSubscription.Name}/$DeadLetterQueue";
            }

            if (_currentQueue != null)
            {
                dlqPath = $"{_currentQueue.Name}/$DeadLetterQueue";
            }
            
            var buttonResult = await MessageBoxHelper.ShowConfirmation(
                "Transferring messages from DLQ",
                $"Are you sure you would like to transfer ALL the messages on {dlqPath}?");
            
            // Because buttonResult can be None or No
            if (buttonResult != ButtonResult.Yes)
            {
                CurrentMessage = null;
                return;
            }

            LoggingService.Log($"Transferred ALL messages in {dlqPath}... (might take some time)");
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
            }

            if (CurrentQueue != null)
            {
                var connectionString = CurrentQueue.ServiceBus.ConnectionString;
                purgedCount = await _queueHelper.PurgeMessages(connectionString, _currentQueue.Name, isDlq);
            }

            LoggingService.Log($"Purged {purgedCount} messages in {purgingPath}");
        }

        public async Task Refresh()
        {
            await FetchMessages();
            RefreshTabHeaders();
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
    }
}