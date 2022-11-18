using System.Collections.Generic;
using System.Threading.Tasks;
using PurpleExplorer.Models;
using Message = PurpleExplorer.Models.Message;
using AzureMessage = Microsoft.Azure.ServiceBus.Message;

namespace PurpleExplorer.Helpers
{
    public interface IQueueHelper
    {
        Task<IList<ServiceBusQueue>> GetQueues(ServiceBusConnectionString connectionString);
        public Task SendMessage(ServiceBusConnectionString connectionString, string topicPath, string content);
        public Task SendMessage(ServiceBusConnectionString connectionString, string topicPath, AzureMessage message);
        Task<IList<Message>> GetMessages(ServiceBusConnectionString connectionString, string queueName);
        Task<IList<Message>> GetDlqMessages(ServiceBusConnectionString connectionString, string queueName);
        Task DeadletterMessage(ServiceBusConnectionString connectionString, string queue, Message message);
        Task DeleteMessage(ServiceBusConnectionString connectionString, string queue,
            Message message, bool isDlq);
        Task ResubmitDlqMessage(ServiceBusConnectionString connectionString, string queue, Message message);
        Task<long> PurgeMessages(ServiceBusConnectionString connectionString, string queue, bool isDlq);
        Task<long> TransferDlqMessages(ServiceBusConnectionString connectionString, string queue);
    }
}