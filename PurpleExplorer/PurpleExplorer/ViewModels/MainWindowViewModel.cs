using System.Collections.Generic;
using System.Collections.ObjectModel;
using PurpleExplorer.Models;

namespace PurpleExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<ServiceBusResource> ConnectedServiceBuses { get; }
        public ObservableCollection<Message> Messages { get; }

        public MainWindowViewModel()
        {
            ConnectedServiceBuses =
                new ObservableCollection<ServiceBusResource>(new[]
                {
                    new ServiceBusResource
                    {
                        Name = "ServiceBus1",
                        Topics = new ObservableCollection<ServiceBusTopic>(new []
                        {
                            new ServiceBusTopic
                            {
                                Name = "Topic1-1",
                                Subscriptions = new ObservableCollection<ServiceBusSubscription>(new []
                                {
                                    new ServiceBusSubscription {Name = "Subscription-1"},
                                    new ServiceBusSubscription {Name = "Subscription-2"}
                                })
                            }
                        })
                    }
                });
            
            Messages = new ObservableCollection<Message>(GenerateMockMessages());
        }
        private IEnumerable<Message> GenerateMockMessages()
        {
            var mockMessages = new List<Message>()
            {
                new Message()
                {
                    Content = "Test Message 1",
                    Size = 1
                },
                new Message()
                {
                    Content = "Test Message 2",
                    Size = 2
                },
                new Message()
                {
                    Content = "Test Message 3",
                    Size = 3
                }
            };
            return mockMessages;
        }
    }
}