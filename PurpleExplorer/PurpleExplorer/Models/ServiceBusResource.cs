using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PurpleExplorer.Models
{
    public class ServiceBusResource 
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public ObservableCollection<ServiceBusTopic> Topics { get; set; }
    }
}