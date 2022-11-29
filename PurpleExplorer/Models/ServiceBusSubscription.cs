using Microsoft.Azure.ServiceBus.Management;

namespace PurpleExplorer.Models;

public class ServiceBusSubscription : MessageCollection
{
    public string Name { get; set; }
       
    public ServiceBusTopic Topic { get; set; }

    public ServiceBusSubscription(SubscriptionRuntimeInfo subscription)
        : base(subscription.MessageCountDetails.ActiveMessageCount, subscription.MessageCountDetails.DeadLetterMessageCount)
    {
        Name = subscription.SubscriptionName;
    }
}