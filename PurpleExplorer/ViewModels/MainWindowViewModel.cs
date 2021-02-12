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
using PurpleExplorer.Services;
using Splat;

namespace PurpleExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IServiceBusHelper _serviceBusHelper;
        private readonly ILoggingService _loggingService;
        private string _messageTabHeader;
        private string _dlqTabHeader;
        private ServiceBusSubscription _currentSubscription;
        private ServiceBusTopic _currentTopic;
        private Message _currentMessage;
        private string _connectionString;
        private IObservable<bool> _sendMessageEnabled;

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

        public ILoggingService LoggingService
        {
            get => _loggingService;
        }

        public Version AppVersion => Assembly.GetExecutingAssembly().GetName().Version;
        public string AppVersionText { get; set; }

        public IObservable<bool> SendMessageEnabled
        {
            get => _sendMessageEnabled;
            set => this.RaiseAndSetIfChanged(ref _sendMessageEnabled, value);
        }
        
        public MainWindowViewModel(IServiceBusHelper serviceBusHelper = null, ILoggingService loggingService = null)
        {
            _loggingService = loggingService ?? Locator.Current.GetService<ILoggingService>();
            _serviceBusHelper = serviceBusHelper ?? Locator.Current.GetService<IServiceBusHelper>();

            ConnectedServiceBuses = new ObservableCollection<ServiceBusResource>();

            _sendMessageEnabled = this.WhenAnyValue<MainWindowViewModel, bool, ServiceBusTopic>(
                x => x.CurrentTopic,
                x => x != null && x.Subscriptions?.Count > 0
            );

            SetTabHeaders();

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

                var namespaceInfo = await _serviceBusHelper.GetNamespaceInfo(ConnectionString);
                var topics = await _serviceBusHelper.GetTopics(ConnectionString);

                var newResource = new ServiceBusResource
                {
                    Name = namespaceInfo.Name,
                    ConnectionString = this.ConnectionString
                };

                newResource.AddTopics(topics.ToArray());
                ConnectedServiceBuses.Add(newResource);
                LoggingService.Log("Connected to Service Bus: " + namespaceInfo.Name);
            }
            catch (ArgumentException)
            {
                await MessageBoxHelper.ShowError("The connection string is invalid.");
            }
        }

        public async Task SetDlqMessages()
        {
            CurrentSubscription.DlqMessages.Clear();
            var dlqMessages =
                await _serviceBusHelper.GetDlqMessages(CurrentSubscription.Topic.ServiceBus.ConnectionString,
                    CurrentSubscription.Topic.Name, CurrentSubscription.Name);
            CurrentSubscription.DlqMessages.AddRange(dlqMessages);
        }

        public async Task SetSubscriptionMessages()
        {
            CurrentSubscription.Messages.Clear();
            var messages =
                await _serviceBusHelper.GetMessagesBySubscription(CurrentSubscription.Topic.ServiceBus.ConnectionString,
                    CurrentSubscription.Topic.Name,
                    CurrentSubscription.Name);
            CurrentSubscription.Messages.AddRange(messages);
        }

        public async Task RefreshMessageCount()
        {
            var connectionString = CurrentTopic.ServiceBus.ConnectionString;
            var topicPath = CurrentTopic.Name;
            var subscriptionName = CurrentSubscription.Name;
            var runtimeInfo =
                await _serviceBusHelper.GetSubscriptionRuntimeInfo(connectionString, topicPath, subscriptionName);

            CurrentSubscription.UpdateMessageCountDetails(runtimeInfo.MessageCountDetails);
            SetTabHeaders();
        }

        public void SetTabHeaders()
        {
            if (CurrentSubscription == null)
            {
                MessagesTabHeader = "Messages";
                DlqTabHeader = "Dead-letter";
            }
            else
            {
                MessagesTabHeader = $"Messages ({CurrentSubscription.MessageCount})";
                DlqTabHeader = $"Dead-letter ({CurrentSubscription.DlqCount})";
            }
        }

        public async void AddMessage()
        {
            var viewModal = new AddMessageWindowViewModal();

            var topicName = CurrentSubscription == null ? CurrentTopic.Name : CurrentSubscription.Topic.Name;
            var returnedViewModal =
                await ModalWindowHelper.ShowModalWindow<AddMessageWindow, AddMessageWindowViewModal>(viewModal);

            if (returnedViewModal.Cancel)
            {
                return;
            }

            var messageText = returnedViewModal.Message.Trim();
            var connectionString = CurrentTopic.ServiceBus.ConnectionString;
            if (!string.IsNullOrEmpty(messageText))
            {
                LoggingService.Log("Sending message...");
                await _serviceBusHelper.SendMessage(connectionString, topicName, messageText);
                LoggingService.Log("Message sent");
            }
        }

        public async void PurgeMessages(string isDlqText)
        {
            var isDlq = Convert.ToBoolean(isDlqText);
            var subscriptionPathText =
                isDlq
                    ? $"{_currentTopic.Name}/{_currentSubscription.Name}/$DeadLetterQueue"
                    : $"{_currentTopic.Name}/{_currentSubscription.Name}";

            var buttonResult = await MessageBoxHelper.ShowConfirmation(
                $"Purging messages from {subscriptionPathText}",
                $"Are you sure you would like to purge ALL the messages?");

            // Because buttonResult can be None or No
            if (buttonResult != ButtonResult.Yes)
            {
                CurrentMessage = null;
                return;
            }

            LoggingService.Log($"Purging ALL messages in {subscriptionPathText}... (might take some time)");
            var connectionString = CurrentTopic.ServiceBus.ConnectionString;
            var purgedCount = await _serviceBusHelper.PurgeMessages(connectionString, _currentTopic.Name,
                _currentSubscription.Name, isDlq);
            LoggingService.Log($"Purged {purgedCount} messages in {subscriptionPathText}");
        }

        public async Task RefreshMessages()
        {
            LoggingService.Log("Fetching messages...");

            await Task.WhenAll(
                SetSubscriptionMessages(),
                SetDlqMessages(),
                RefreshMessageCount()
            );

            LoggingService.Log("Fetched messages");
        }

        public async void SetSelectedSubscription(ServiceBusSubscription subscription)
        {
            CurrentSubscription = subscription;
            CurrentTopic = subscription.Topic;
            LoggingService.Log("Subscription selected: " + subscription.Name);
            
            await RefreshMessages();
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

        public void ClearSelection()
        {
            CurrentSubscription = null;
            CurrentTopic = null;
            CurrentMessage = null;
            SetTabHeaders();
        }
    }
}