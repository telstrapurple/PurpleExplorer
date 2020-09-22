using System.Collections.ObjectModel;
using Microsoft.Azure.ServiceBus.Management;

namespace PurpleExplorer.Models
{
    public class ServiceBusSubscription
    {
        public string Name { get; set; }
        public long MessageCount { get; set; }
        public long DLQCount { get; set; }
        public ServiceBusTopic Topic { get; set; }
        public ObservableCollection<Message> Messages { get; }
        public ObservableCollection<Message> DlqMessages { get; }

        public ServiceBusSubscription(SubscriptionRuntimeInfo subscription)
        {
            Messages = new ObservableCollection<Message>();
            DlqMessages = new ObservableCollection<Message>();
            Name = subscription.SubscriptionName;
            MessageCount = subscription.MessageCountDetails.ActiveMessageCount;
            DLQCount = subscription.MessageCountDetails.DeadLetterMessageCount;
        }

        public void UpdateMessageDlqCount(SubscriptionRuntimeInfo subscription)
        {
            MessageCount = subscription.MessageCountDetails.ActiveMessageCount;
            DLQCount = subscription.MessageCountDetails.DeadLetterMessageCount;
        }
    }
}