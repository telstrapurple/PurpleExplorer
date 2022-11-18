using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Management;
using PurpleExplorer.Models;
using AzureMessage = Microsoft.Azure.ServiceBus.Message;

namespace PurpleExplorer.Helpers
{
    public interface ITopicHelper
    {
        public Task<NamespaceInfo> GetNamespaceInfo(ServiceBusConnectionString connectionString);
        public Task<IList<ServiceBusTopic>> GetTopicsAndSubscriptions(ServiceBusConnectionString connectionString);
        public Task<ServiceBusTopic> GetTopic(ServiceBusConnectionString connectionString, string topicPath, bool retrieveSubscriptions);
        public Task<IList<ServiceBusSubscription>> GetSubscriptions(ServiceBusConnectionString connectionString, string topicPath);
        public Task<ServiceBusSubscription> GetSubscription(ServiceBusConnectionString connectionString, string topicPath, string subscriptionName);
        public Task<IList<Message>> GetDlqMessages(ServiceBusConnectionString connectionString, string topic, string subscription);
        public Task<IList<Models.Message>> GetMessagesBySubscription(ServiceBusConnectionString connectionString, string topicName, string subscriptionName);
        public Task SendMessage(ServiceBusConnectionString connectionString, string topicPath, string content);
        public Task SendMessage(ServiceBusConnectionString connectionString, string topicPath, AzureMessage message);
        public Task DeleteMessage(ServiceBusConnectionString connectionString, string topicPath, string subscriptionPath, Message message, bool isDlq);
        public Task<long> PurgeMessages(ServiceBusConnectionString connectionString, string topicPath, string subscriptionPath, bool isDlq);
        public Task<long> TransferDlqMessages(ServiceBusConnectionString connectionString, string topicPath, string subscriptionPath);
        public Task ResubmitDlqMessage(ServiceBusConnectionString connectionString, string topicPath, string subscriptionPath,
            Message message);
        public Task DeadletterMessage(ServiceBusConnectionString connectionString, string topicPath, string subscriptionPath,
            Message message);
    }
}