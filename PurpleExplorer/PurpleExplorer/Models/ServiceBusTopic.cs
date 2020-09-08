using System;
using System.Collections.ObjectModel;

namespace PurpleExplorer.Models
{
    public class ServiceBusTopic
    {
        public string Name { get; set; }
        public ObservableCollection<ServiceBusSubscription> Subscriptions { get; set; }
    }
}