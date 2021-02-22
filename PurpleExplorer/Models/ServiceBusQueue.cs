using Microsoft.Azure.ServiceBus.Management;

namespace PurpleExplorer.Models
{
    public class ServiceBusQueue : MessageCollection
    {
        public string Name { get; set; }
        public ServiceBusResource ServiceBus { get; set; }
        
        public ServiceBusQueue(QueueRuntimeInfo runtimeInfo)
            : base(runtimeInfo.MessageCountDetails.ActiveMessageCount, runtimeInfo.MessageCountDetails.DeadLetterMessageCount)
        {
            
        }
    }
}