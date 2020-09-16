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

        public MainWindowViewModel(IServiceBusHelper serviceBusHelper = null)
        {
            _serviceBusHelper = serviceBusHelper ?? Locator.Current.GetService<IServiceBusHelper>();
            ConnectedServiceBuses = new ObservableCollection<ServiceBusResource>();
            // this.WhenAnyValue(x => x.CurrentSubscription)
            //     .Subscribe(x =>
            //     {
            //         if (x != null)
            //         {
            //             CurrentSubscriptionUpdated();
            //         }
            //     });

            SetTabHeaders();
        }

        public async void ConnectionBtnPopupCommand()
        {
            var viewModel = new ConnectionStringWindowViewModel();

            var returnedViewModel =
                await ModalWindowHelper.ShowModalWindow<ConnectionStringWindow, ConnectionStringWindowViewModel>(
                    viewModel, 700, 100);
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

        // public void ClearAllMessages()
        // {
        //     Messages.Clear();
        //     DlqMessages.Clear();
        // }

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
            if (_currentSubscription == null)
            {
                return;
            }

            var viewModal = new AddMessageWindowViewModal();

            var returnedViewModal =
                await ModalWindowHelper.ShowModalWindow<AddMessageWindow, AddMessageWindowViewModal>(viewModal, 700,
                    100);

            var message = returnedViewModal.Message.Trim();

            await _serviceBusHelper.SendTopicMessage(_connectionString, CurrentSubscription.Topic.Name, message);
        }

        // public async void CurrentSubscriptionUpdated()
        // {
        //    
        // }

        public async void SetSelectedSubscription(ServiceBusSubscription subscription)
        {
            CurrentSubscription = subscription;
            await Task.WhenAll(
                SetSubscripitonMessages(),
                SetDlqMessages());
            
            SetTabHeaders();
        }
        
        public void ClearSelectedSubscription()
        {
            CurrentSubscription = null;
            SetTabHeaders();

            // mainWindowViewModel.ClearAllMessages();
            // mainWindowViewModel.SetTabHeaders();
        }
    }
}