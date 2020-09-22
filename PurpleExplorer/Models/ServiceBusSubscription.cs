using System.Collections.ObjectModel;
using Microsoft.Azure.ServiceBus.Management;
using ReactiveUI;

namespace PurpleExplorer.Models
{
    public class ServiceBusSubscription: ReactiveObject
    {
        public string Name { get; set; }
        private long _messageCount;
        public long MessageCount
        {
            get => _messageCount;
            set => this.RaiseAndSetIfChanged(ref _messageCount, value);
        }
        private long _dlqCount;
        public long DlqCount
        {
            get => _dlqCount;
            set => this.RaiseAndSetIfChanged(ref _dlqCount, value);
        }
        public ServiceBusTopic Topic { get; set; }
        public ObservableCollection<Message> Messages { get; }
        public ObservableCollection<Message> DlqMessages { get; }

        public ServiceBusSubscription(SubscriptionRuntimeInfo subscription)
        {
            Messages = new ObservableCollection<Message>();
            DlqMessages = new ObservableCollection<Message>();
            Name = subscription.SubscriptionName;
            UpdateMessageCountDetails(subscription.MessageCountDetails);
        }

        public void UpdateMessageCountDetails(MessageCountDetails messageCountDetails)
        {
            MessageCount = messageCountDetails.ActiveMessageCount;
            DlqCount = messageCountDetails.DeadLetterMessageCount;
        }
    }
}