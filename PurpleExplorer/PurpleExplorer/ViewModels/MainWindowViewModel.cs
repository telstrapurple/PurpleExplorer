using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using PurpleExplorer.Models;

namespace PurpleExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<ServiceBusResource> ConnectedServiceBuses { get; }

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
        }
    }
}