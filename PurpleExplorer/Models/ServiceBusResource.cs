using System;
using System.Collections.ObjectModel;

namespace PurpleExplorer.Models
{
    public class ServiceBusResource 
    {
        public string Name { get; set; }
        public DateTime CreatedTime { get; set; }
        public string ConnectionString { get; set; }
        public ObservableCollection<ServiceBusQueue> Queues { get; private set; }
        public ObservableCollection<ServiceBusTopic> Topics { get; private set; }
        
        public void AddTopics(params ServiceBusTopic[] topics)
        {
            Topics ??= new ObservableCollection<ServiceBusTopic>();

            foreach (var topic in topics)
            {
                topic.ServiceBus = this;
                Topics.Add(topic);
            }
        }
        
        public void AddQueues(params ServiceBusQueue[] queues)
        {
            Queues ??= new ObservableCollection<ServiceBusQueue>();

            foreach (var queue in queues)
            {
                queue.ServiceBus = this;
                Queues.Add(queue);
            }
        }

        public override bool Equals(object? obj)
        {
            var comparingResource = obj as ServiceBusResource;
            return comparingResource != null && Name.Equals(comparingResource.Name) && CreatedTime.Equals(comparingResource.CreatedTime);
        }
    }
}