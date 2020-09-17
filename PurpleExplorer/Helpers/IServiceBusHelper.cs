using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus.Management;
using PurpleExplorer.Models;

namespace PurpleExplorer.Helpers
{
    public interface IServiceBusHelper
    {
        public Task<NamespaceInfo> GetNamespaceInfo(string connectionString);
        public Task<IList<ServiceBusTopic>> GetTopics(string connectionString);
        public Task<IList<ServiceBusSubscription>> GetSubscriptions(string connectionString, string topicPath);
        public Task<IList<Message>> GetDlqMessages(string connectionString, string topic, string subscription);
        public Task<IList<Models.Message>> GetMessagesBySubscription(string connectionString, string topicName, string subscriptionName);
        public Task SendTopicMessage(string connectionString, string topicPath, string message);
        public void DeleteMessage(string connectionString, string topicPath, string subscriptionPath, Message message, bool isDlq);

    }
}