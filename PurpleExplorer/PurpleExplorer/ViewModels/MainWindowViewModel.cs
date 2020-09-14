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
        private string _connectionString { get; set; }
        private IServiceBusHelper ServiceBusHelper { get; }
        private string _messageTabHeader;

        public ObservableCollection<ServiceBusResource> ConnectedServiceBuses { get; }
        public ObservableCollection<Message> Messages { get; set; }
        public ObservableCollection<Message> DlqMessages { get; }

        public string MessagesTabHeader
        {
            get => _messageTabHeader;
            set => this.RaiseAndSetIfChanged(ref _messageTabHeader, value);
        }

        public string _dlqTabHeader;

        public string DLQTabHeader
        {
            get => _dlqTabHeader;
            set => this.RaiseAndSetIfChanged(ref _dlqTabHeader, value);
        }

        public MainWindowViewModel(IServiceBusHelper serviceBusHelper = null)
        {
            ServiceBusHelper = serviceBusHelper ?? Locator.Current.GetService<IServiceBusHelper>();
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
                var namespaceInfo = await ServiceBusHelper.GetNamespaceInfo(_connectionString);
                var topics = await ServiceBusHelper.GetTopics(_connectionString);

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
                await ServiceBusHelper.GetDlqMessages(_connectionString, subscription.Topic.Name, subscription.Name);
            DlqMessages.AddRange(dlqMessages);

            SetTabHeaders();
        }

        public async Task SetSubscripitonMessages(ServiceBusSubscription subscription)
        {
            Messages.Clear();
            var messages =
                await ServiceBusHelper.GetMessagesBySubscription(_connectionString, subscription.Topic.Name,
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