using System;
using System.Collections.ObjectModel;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using MessageBox.Avalonia.Enums;
using PurpleExplorer.Views;
using Splat;
using ReactiveUI;
using System.Threading.Tasks;
using DynamicData;

namespace PurpleExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<ServiceBusResource> ConnectedServiceBuses { get; }
        public ObservableCollection<Message> Messages { get; set; }
        public ObservableCollection<Message> DlqMessages { get; }
        private IServiceBusHelper ServiceBusHelper { get; }
        private string _connectionString { get; set; }
        public string _messageTabHeader;
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

        private ServiceBusSubscription _currentSubscription;

        public ServiceBusSubscription CurrentSubscription
        {
            get => _currentSubscription;
            set => this.RaiseAndSetIfChanged(ref _currentSubscription, value);
        }

        public MainWindowViewModel(IServiceBusHelper serviceBusHelper = null)
        {
            ServiceBusHelper = serviceBusHelper ?? Locator.Current.GetService<IServiceBusHelper>();
            ConnectedServiceBuses = new ObservableCollection<ServiceBusResource>();
            Messages = new ObservableCollection<Message>();
            DlqMessages = new ObservableCollection<Message>();
            MessagesTabHeader = "Messages";
            DLQTabHeader = "Dead-letter";
            this.WhenAnyValue(x => x.CurrentSubscription)
                .Subscribe(x =>
                {
                    if (x != null)
                    {
                        CurrentSubscriptionUpdated();
                    }
                });
        }

        private void GenerateMockMessages(int count, int dlqCount)
        {
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                Messages.Add(new Message()
                {
                    Content = "Mocked Message " + i,
                    Size = random.Next(1, 1024)
                });
            }

            for (int i = 0; i < dlqCount; i++)
            {
                DlqMessages.Add(new Message()
                {
                    Content = "Mocked Message " + i,
                    Size = random.Next(1, 1024)
                });
            }
        }

        public async void ConnectionBtnPopupCommand()
        {
            var viewModel = new ConnectionStringWindowViewModel();

            var returnedViewModel = await ModalWindowHelper.ShowModalWindow<ConnectionStringWindow, ConnectionStringWindowViewModel>(viewModel, 700, 100);
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
                //GenerateMockMessages(8, 2);
            }
            catch (ArgumentException)
            {
                await MessageBoxHelper.ShowError(ButtonEnum.Ok, "Error", "The connection string is invalid.");
            }
            catch (Exception e)
            {
                await MessageBoxHelper.ShowError(ButtonEnum.Ok, "Error",
                    $"An error has occurred. Please try again. {e}");
            }
        }

        public async Task SetSubscripitonMessages(ServiceBusSubscription subscription)
        {
            var messages = await ServiceBusHelper.GetMessagesBySubscription(_connectionString, subscription.Topic.Name, subscription.Name);
            Messages.Clear();
            Messages.AddRange(messages);

            this.MessagesTabHeader = "Messages (" + messages.Count.ToString() + ")";            
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

            var message = returnedViewModal.Message; 
            
            await ServiceBusHelper.SendTopicMessage(_connectionString, CurrentSubscription.Topic.Name, message);
        }

        public async void CurrentSubscriptionUpdated()
        {
            await SetSubscripitonMessages(CurrentSubscription);
        }
    }
}