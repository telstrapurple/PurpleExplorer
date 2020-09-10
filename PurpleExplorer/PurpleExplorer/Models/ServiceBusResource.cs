using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PurpleExplorer.Models
{
    public class ServiceBusResource 
    {
        public string Name { get; set; }
        public ObservableCollection<ServiceBusTopic> Topics { get; set; }

        public ServiceBusResource(IEnumerable<ServiceBusTopic> topics)
        {
            Topics = new ObservableCollection<ServiceBusTopic>(topics);
        }
    }
}