using System.Collections.ObjectModel;

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

        public ServiceBusSubscription()
        {
            Messages = new ObservableCollection<Message>();
            DlqMessages = new ObservableCollection<Message>();
        }
    }
}