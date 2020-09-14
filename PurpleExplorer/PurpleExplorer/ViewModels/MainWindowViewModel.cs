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

        public ObservableCollection<ServiceBusResource> ConnectedServiceBuses { get; }
        public ObservableCollection<Message> Messages { get; set; }
        public ObservableCollection<Message> DlqMessages { get; }
        public string MessagesTabHeader
        {
            get => _messageTabHeader;
            set => this.RaiseAndSetIfChanged(ref _messageTabHeader, value);
        }
        public string DLQTabHeader
        {
            get => _dlqTabHeader;
            set => this.RaiseAndSetIfChanged(ref _dlqTabHeader, value);
        }

        public MainWindowViewModel(IServiceBusHelper serviceBusHelper = null)
        {
            _serviceBusHelper = serviceBusHelper ?? Locator.Current.GetService<IServiceBusHelper>();
            ConnectedServiceBuses = new ObservableCollection<ServiceBusResource>();
            Messages = new ObservableCollection<Message>();
            DlqMessages = new ObservableCollection<Message>();

            SetTabHeaders();
        }

        public async void BtnPopupCommand()
        {
            var viewModel = new ConnectionStringWindowViewModel();

            var returnedViewModel =
                await ModalWindowHelper.ShowModalWindow<ConnectionStringWindow, ConnectionStringWindowViewModel>(
                    viewModel, 700, 100);
            _connectionString = returnedViewModel.ConnectionString;

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

        public void ClearAllMessages()
        {
            Messages.Clear();
            DlqMessages.Clear();
        }

        public async Task SetDlqMessages(ServiceBusSubscription subscription)
        {
            DlqMessages.Clear();
            var dlqMessages =
                await _serviceBusHelper.GetDlqMessages(_connectionString, subscription.Topic.Name, subscription.Name);
            DlqMessages.AddRange(dlqMessages);

            SetTabHeaders();
        }

        public async Task SetSubscripitonMessages(ServiceBusSubscription subscription)
        {
            Messages.Clear();
            var messages =
                await _serviceBusHelper.GetMessagesBySubscription(_connectionString, subscription.Topic.Name,
                    subscription.Name);
            Messages.AddRange(messages);

            SetTabHeaders();
        }

        public void SetTabHeaders()
        {
            MessagesTabHeader = $"Messages ({Messages.Count})";
            DLQTabHeader = $"Dead-letter ({DlqMessages.Count})";
        }
    }
}