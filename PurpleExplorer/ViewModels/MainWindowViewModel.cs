using System;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.Views;
using ReactiveUI;
using System.Threading.Tasks;
using MessageBox.Avalonia.Enums;
using PurpleExplorer.Services;
using Splat;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace PurpleExplorer.ViewModels
{
    [DataContract]
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
        private IList<string> _savedConnectionStrings;

        public ObservableCollection<ServiceBusResource> ConnectedServiceBuses { get; }
        
        [DataMember]
        public ObservableCollection<string> SavedConnectionStrings { get; set; }
        
        [DataMember]
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

        public string LogText
        {
            get => _loggingService.Logs;
        }
        public MainWindowViewModel(IServiceBusHelper serviceBusHelper = null, ILoggingService loggingService = null)
        {
            _loggingService = loggingService ?? Locator.Current.GetService<ILoggingService>();
            _serviceBusHelper = serviceBusHelper ?? Locator.Current.GetService<IServiceBusHelper>();

            ConnectedServiceBuses = new ObservableCollection<ServiceBusResource>();
            SavedConnectionStrings = new ObservableCollection<string>();
            
            SetTabHeaders();
        }

        public async void ConnectionBtnPopupCommand()
        {
            var viewModel = new ConnectionStringWindowViewModel() { ConnectionString = this.ConnectionString, SavedConnectionStrings = this.SavedConnectionStrings };

            var returnedViewModel =
                await ModalWindowHelper.ShowModalWindow<ConnectionStringWindow, ConnectionStringWindowViewModel>(
                    viewModel);

            if (returnedViewModel.Cancel)
            {
                return;
            }

            ConnectionString = returnedViewModel.ConnectionString?.Trim();
            SavedConnectionStrings = returnedViewModel.SavedConnectionStrings;

            if (string.IsNullOrEmpty(ConnectionString))
            {
                return;
            }

            try
            {
                Log("Connecting...");

                var namespaceInfo = await _serviceBusHelper.GetNamespaceInfo(ConnectionString);
                var topics = await _serviceBusHelper.GetTopics(ConnectionString);

                var newResource = new ServiceBusResource
                {
                    Name = namespaceInfo.Name,
                    ConnectionString = this.ConnectionString
                };

                newResource.AddTopics(topics.ToArray());
                ConnectedServiceBuses.Add(newResource);
                Log("Connected to Service Bus: " + namespaceInfo.Name);
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

        public void SetTabHeaders()
        {
            if (CurrentSubscription == null)
            {
                MessagesTabHeader = "Messages";
                DlqTabHeader = "Dead-letter";
            }
            else
            {
                MessagesTabHeader = $"Messages ({CurrentSubscription.Messages.Count})";
                DlqTabHeader = $"Dead-letter ({CurrentSubscription.DlqMessages.Count})";
            }
        }

        public async void AddMessage()
        {
            if (CurrentSubscription != null || (CurrentTopic != null && CurrentTopic.Subscriptions.Count > 0))
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
                    Log("Sending message...");
                    await _serviceBusHelper.SendTopicMessage(connectionString, topicName, messageText);
                    Log("Message sent");
                }
            }

            if (CurrentTopic != null && CurrentTopic.Subscriptions.Count == 0)
                await MessageBoxHelper.ShowError("Can't send a message to a Topic without any subscriptions.");
        }

        private void Log(string message)
        {
            _loggingService.Log(message);
            this.RaisePropertyChanged(nameof(LogText));
        }

        public async void DeleteMessage()
        {
            var buttonResult = await MessageBoxHelper.ShowConfirmation(
                $"Deleting message from {_currentTopic.Name}/{_currentSubscription.Name}",
                $"Are you sure you would like to delete the message with ID: {_currentMessage.MessageId}");

            // Because buttonResult can be None or No
            if (buttonResult != ButtonResult.Yes)
            {
                CurrentMessage = null;
                return;
            }

            Log($"Deleting message {_currentMessage.MessageId}... (might take some seconds)");
            var connectionString = CurrentTopic.ServiceBus.ConnectionString;
            await _serviceBusHelper.DeleteMessage(connectionString, _currentTopic.Name, _currentSubscription.Name,
                _currentMessage, _currentMessage.IsDlq);
            Log($"Message deleted, MessageId: {_currentMessage.MessageId}");
            CurrentMessage = null;
        }

        public async Task RefreshMessages()
        {
            Log("Fetching messages...");

            await Task.WhenAll(
                SetSubscriptionMessages(),
                SetDlqMessages());

            Log("Fetched messages");

            SetTabHeaders();
        }

        public async void SetSelectedSubscription(ServiceBusSubscription subscription)
        {
            CurrentSubscription = subscription;
            CurrentTopic = subscription.Topic;

            await RefreshMessages();
            Log("Subscription selected: " + subscription.Name);
        }

        public void SetSelectedTopic(ServiceBusTopic selectedTopic)
        {
            CurrentTopic = selectedTopic;
            Log("Topic selected: " + selectedTopic.Name);
        }

        public void SetSelectedMessage(Message message)
        {
            CurrentMessage = message;
            Log("Message selected: " + message.MessageId);
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