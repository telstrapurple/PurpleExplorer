using System.Collections.ObjectModel;

namespace PurpleExplorer.Models
{
    public class ServiceBusResource 
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public ObservableCollection<ServiceBusTopic> Topics { get; set; }
        
        public void AddTopics(params ServiceBusTopic[] topics)
        {
            Topics ??= new ObservableCollection<ServiceBusTopic>();

            foreach (var topic in topics)
            {
                topic.ServiceBus = this;
                Topics.Add(topic);
            }
        }
    }
}