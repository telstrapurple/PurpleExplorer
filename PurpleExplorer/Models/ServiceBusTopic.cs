using System.Collections.ObjectModel;

namespace PurpleExplorer.Models
{
    public class ServiceBusTopic
    {
        public string Name { get; set; }
        public ObservableCollection<ServiceBusSubscription> Subscriptions { get; private set; }
        public ServiceBusResource ServiceBus { get; set; }

        public void AddSubscriptions(params ServiceBusSubscription[] subscriptions)
        {
            Subscriptions ??= new ObservableCollection<ServiceBusSubscription>();

            foreach (var subscription in subscriptions)
            {
                subscription.Topic = this;
                Subscriptions.Add(subscription);
            }
        }
    }
}