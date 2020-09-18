using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using DynamicData;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.Views;
using ReactiveUI;
using System.Threading.Tasks;
using MessageBox.Avalonia.Enums;
using PurpleExplorer.Services;

namespace PurpleExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IServiceBusHelper _serviceBusHelper;
        private ILoggingService _loggingService;
        private string _messageTabHeader;
        private string _dlqTabHeader;
        private ServiceBusSubscription _currentSubscription;
        private ServiceBusTopic _currentTopic;
        private Message _currentMessage;

        public ObservableCollection<ServiceBusResource> ConnectedServiceBuses { get; }

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

        public ReactiveCommand<Unit, Unit> Delete { get; }

        public MainWindowViewModel(IServiceBusHelper serviceBusHelper = null, ILoggingService loggingService = null)
        {
            _loggingService = loggingService;
            _serviceBusHelper = serviceBusHelper;

            ConnectedServiceBuses = new ObservableCollection<ServiceBusResource>();

            var deleteEnabled =
                this.WhenAnyValue<MainWindowViewModel, bool, Message>(x => x.CurrentMessage, x => x != null);
            Delete = ReactiveCommand.Create(() => DeleteMessage(), deleteEnabled);

            SetTabHeaders();
        }

        public async void ConnectionBtnPopupCommand()
        {
            var viewModel = new ConnectionStringWindowViewModel();

            var returnedViewModel =
                await ModalWindowHelper.ShowModalWindow<ConnectionStringWindow, ConnectionStringWindowViewModel>(
                    viewModel);
            var connectionString = returnedViewModel.ConnectionString?.Trim();

            if (returnedViewModel.Cancel)
            {
                return;
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            try
            {
                Log("Connecting...");

                var namespaceInfo = await _serviceBusHelper.GetNamespaceInfo(connectionString);
                var topics = await _serviceBusHelper.GetTopics(connectionString);

                var newResource = new ServiceBusResource
                {
                    Name = namespaceInfo.Name,
                    ConnectionString = connectionString
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

                if (!returnedViewModal.Cancel)
                {
                    var messageText = returnedViewModal.Message.Trim();
                    var connectionString = CurrentTopic.ServiceBus.ConnectionString;
                    if (!string.IsNullOrEmpty(messageText))
                    {
                        await _serviceBusHelper.SendTopicMessage(connectionString, topicName, messageText);
                        Log("Message added");
                    }
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
                $"Are you sure you would like to delete the message with the content: \n {_currentMessage.Content}");

            if (buttonResult == ButtonResult.No)
            {
                CurrentMessage = null;
                return;
            }

            var connectionString = CurrentTopic.ServiceBus.ConnectionString;
             _serviceBusHelper.DeleteMessage(connectionString, _currentTopic.Name, _currentSubscription.Name,
                _currentMessage, _currentMessage.IsDlq);
            CurrentMessage = null;
            Log("Message deleted.");
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