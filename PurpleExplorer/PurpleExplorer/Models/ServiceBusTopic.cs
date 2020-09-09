using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PurpleExplorer.Models
{
    public class ServiceBusTopic
    {
        public string Name { get; set; }
        public ObservableCollection<ServiceBusSubscription> Subscriptions { get; set; }

        public ServiceBusTopic(IEnumerable<ServiceBusSubscription> subscriptions)
        {
            Subscriptions = new ObservableCollection<ServiceBusSubscription>(subscriptions);
        }
    }
}