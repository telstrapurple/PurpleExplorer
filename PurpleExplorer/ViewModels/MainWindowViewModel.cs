using System;
using System.Collections.ObjectModel;
using DynamicData;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.Views;
using Splat;
using ReactiveUI;
using System.Threading.Tasks;

namespace PurpleExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _connectionString;
        private readonly IServiceBusHelper _serviceBusHelper;
        private string _messageTabHeader;
        private string _dlqTabHeader;
        private ServiceBusSubscription _currentSubscription;
        private ServiceBusTopic _currentTopic;

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

        public MainWindowViewModel(IServiceBusHelper serviceBusHelper = null)
        {
            _serviceBusHelper = serviceBusHelper ?? Locator.Current.GetService<IServiceBusHelper>();
            ConnectedServiceBuses = new ObservableCollection<ServiceBusResource>();

            SetTabHeaders();
        }

        public async void ConnectionBtnPopupCommand()
        {
            var viewModel = new ConnectionStringWindowViewModel();

            var returnedViewModel =
                await ModalWindowHelper.ShowModalWindow<ConnectionStringWindow, ConnectionStringWindowViewModel>(
                    viewModel);
            _connectionString = returnedViewModel.ConnectionString.Trim();

            if (string.IsNullOrEmpty(_connectionString))
            {
                return;
            }

            try
            {
                var namespaceInfo = await _serviceBusHelper.GetNamespaceInfo(_connectionString);
                var topics = await _serviceBusHelper.GetTopics(_connectionString);

                var newResource = new ServiceBusResource
                {
                    Name = namespaceInfo.Name,
                    ConnectionString = _connectionString,
                    Topics = new ObservableCollection<ServiceBusTopic>(topics)
                };

                ConnectedServiceBuses.Add(newResource);
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
                await _serviceBusHelper.GetDlqMessages(_connectionString, CurrentSubscription.Topic.Name, CurrentSubscription.Name);
            CurrentSubscription.DlqMessages.AddRange(dlqMessages);
        }

        public async Task SetSubscripitonMessages()
        {
            CurrentSubscription.Messages.Clear();
            var messages =
                await _serviceBusHelper.GetMessagesBySubscription(_connectionString, CurrentSubscription.Topic.Name,
                    CurrentSubscription.Name);
            CurrentSubscription.Messages.AddRange(messages);
        }

        public void SetTabHeaders()
        {
            if (CurrentSubscription == null)
            {
                MessagesTabHeader = $"Messages";
                DlqTabHeader = $"Dead-letter";
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
                var message = returnedViewModal.Message.Trim();
                await _serviceBusHelper.SendTopicMessage(_connectionString, topicName, message);
            }

            if (CurrentTopic != null && CurrentTopic.Subscriptions.Count == 0)
                await MessageBoxHelper.ShowError("Can't send a message to a Topic without any subscriptions.");
        }

        public async void SetSelectedSubscription(ServiceBusSubscription subscription)
        {
            CurrentSubscription = subscription;
            CurrentTopic = subscription.Topic;

            await Task.WhenAll(
                SetSubscripitonMessages(),
                SetDlqMessages());
            
            SetTabHeaders();
        }

        public void SetSelectedTopic(ServiceBusTopic selectedTopic)
        {
            CurrentTopic = selectedTopic;
        }

        public void ClearSelection()
        {
            CurrentSubscription = null;
            CurrentTopic = null;
            SetTabHeaders();
        }
    }
}