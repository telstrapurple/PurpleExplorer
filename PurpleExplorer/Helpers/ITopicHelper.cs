using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Management;
using PurpleExplorer.Models;
using AzureMessage = Microsoft.Azure.ServiceBus.Message;

namespace PurpleExplorer.Helpers
{
    public interface ITopicHelper
    {
        public Task<NamespaceInfo> GetNamespaceInfo(string connectionString);
        public Task<IList<ServiceBusTopic>> GetTopicsAndSubscriptions(string connectionString);
        public Task<ServiceBusTopic> GetTopic(string connectionString, string topicPath, bool retrieveSubscriptions);
        public Task<IList<ServiceBusSubscription>> GetSubscriptions(string connectionString, string topicPath);
        public Task<ServiceBusSubscription> GetSubscription(string connectionString, string topicPath, string subscriptionName);
        public Task<IList<Message>> GetDlqMessages(string connectionString, string topic, string subscription);
        public Task<IList<Models.Message>> GetMessagesBySubscription(string connectionString, string topicName, string subscriptionName);
        public Task SendMessage(string connectionString, string topicPath, string content);
        public Task SendMessage(string connectionString, string topicPath, AzureMessage message);
        public Task DeleteMessage(string connectionString, string topicPath, string subscriptionPath, Message message, bool isDlq);
        public Task<long> PurgeMessages(string connectionString, string topicPath, string subscriptionPath, bool isDlq);
        public Task<long> TransferDlqMessages(string connectionString, string topicPath, string subscriptionPath);
        public Task ResubmitDlqMessage(string connectionString, string topicPath, string subscriptionPath,
            Message message);
        public Task DeadletterMessage(string connectionString, string topicPath, string subscriptionPath,
            Message message);
    }
}