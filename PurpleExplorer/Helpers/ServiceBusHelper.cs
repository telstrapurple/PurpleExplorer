using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using PurpleExplorer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message = PurpleExplorer.Models.Message;
using AzureMessage = Microsoft.Azure.ServiceBus.Message;

namespace PurpleExplorer.Helpers
{
    public class ServiceBusHelper : IServiceBusHelper
    {
        private int _maxMessageCount = 100;

        public async Task<IList<ServiceBusTopic>> GetTopics(string connectionString)
        {
            IList<ServiceBusTopic> topics = new List<ServiceBusTopic>();
            var client = new ManagementClient(connectionString);
            var busTopics = await client.GetTopicsAsync();
            await client.CloseAsync();

            await Task.WhenAll(busTopics.Select(async t =>
            {
                var topicName = t.Path;
                var subscriptions = await GetSubscriptions(connectionString, topicName);

                var newTopic = new ServiceBusTopic
                {
                    Name = topicName
                };

                newTopic.AddSubscriptions(subscriptions.ToArray());
                topics.Add(newTopic);
            }));

            return topics;
        }

        public async Task<SubscriptionRuntimeInfo> GetSubscriptionRuntimeInfo(string connectionString,
            string topicPath, string subscriptionName)
        {
            ManagementClient client = new ManagementClient(connectionString);
            var runtimeInfo = await client.GetSubscriptionRuntimeInfoAsync(topicPath, subscriptionName);
            await client.CloseAsync();

            return runtimeInfo;
        }

        public async Task<IList<ServiceBusSubscription>> GetSubscriptions(string connectionString, string topicPath)
        {
            IList<ServiceBusSubscription> subscriptions = new List<ServiceBusSubscription>();
            var client = new ManagementClient(connectionString);
            var topicSubscription = await client.GetSubscriptionsRuntimeInfoAsync(topicPath);
            await client.CloseAsync();

            foreach (var sub in topicSubscription)
            {
                subscriptions.Add(new ServiceBusSubscription(sub));
            }

            return subscriptions;
        }

        public async Task<IList<Message>> GetMessagesBySubscription(string connectionString, string topicName,
            string subscriptionName)
        {
            var path = EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName);

            var messageReceiver = new MessageReceiver(connectionString, path, ReceiveMode.PeekLock);
            var subscriptionMessages = await messageReceiver.PeekAsync(_maxMessageCount);
            await messageReceiver.CloseAsync();
            
            var result = subscriptionMessages.Select(message => new Message(message, false)).ToList();
            return result;
        }

        public async Task<IList<Message>> GetDlqMessages(string connectionString, string topic, string subscription)
        {
            var path = EntityNameHelper.FormatSubscriptionPath(topic, subscription);
            var deadletterPath = EntityNameHelper.FormatDeadLetterPath(path);
            
            var receiver = new MessageReceiver(connectionString, deadletterPath, ReceiveMode.PeekLock);
            var receivedMessages = await receiver.PeekAsync(_maxMessageCount);
            await receiver.CloseAsync();

            var result = receivedMessages.Select(message => new Message(message, true)).ToList();
            return result;
        }

        public async Task<NamespaceInfo> GetNamespaceInfo(string connectionString)
        {
            var client = new ManagementClient(connectionString);
            return await client.GetNamespaceInfoAsync();
        }
        
        public async Task SendTopicMessage(string connectionString, string topicPath, string content)
        {
            var message = new AzureMessage() {Body = Encoding.UTF8.GetBytes(content)};
            await SendTopicClientMessage(connectionString, topicPath, message);
        }

        public async Task SendTopicMessage(string connectionString, string topicPath, AzureMessage message)
        {
            await SendTopicClientMessage(connectionString, topicPath, message);
        }
        
        async Task SendTopicClientMessage(string connectionString, string topicPath, AzureMessage messageToSend)
        {
            var topicClient = new TopicClient(connectionString, topicPath);
            await topicClient.SendAsync(messageToSend);
            await topicClient.CloseAsync();
        }

        public async Task DeleteMessage(string connectionString, string topicPath, string subscriptionPath,
            Message message, bool isDlq)
        {
            var path = EntityNameHelper.FormatSubscriptionPath(topicPath, subscriptionPath);
            path = isDlq ? EntityNameHelper.FormatDeadLetterPath(path) : path;

            var receiver = new MessageReceiver(connectionString, path, ReceiveMode.PeekLock);
            
            while (true)
            {
                var messages = await receiver.ReceiveAsync(_maxMessageCount);
                if (messages == null || messages.Count == 0)
                {
                    break;
                }

                var foundMessage = messages.FirstOrDefault(m => m.MessageId.Equals(message.MessageId));
                if (foundMessage != null)
                {
                    await receiver.CompleteAsync(foundMessage.SystemProperties.LockToken);
                    break;
                }
            }

            await receiver.CloseAsync();
        }

        public async Task ResendDlqMessage(string connectionString, string topicPath, string subscriptionPath, Message message)
        {
            var path = EntityNameHelper.FormatSubscriptionPath(topicPath, subscriptionPath);
            var deadletterPath = EntityNameHelper.FormatDeadLetterPath(path);        
            
            //get message
            var receiver = new MessageReceiver(connectionString, deadletterPath, ReceiveMode.PeekLock);
            var azureMessage = await receiver.PeekBySequenceNumberAsync(message.SequenceNumber);
            var clonedMessage = azureMessage.Clone();
            await receiver.CloseAsync();

            //send message
            await SendTopicMessage(connectionString, topicPath, clonedMessage);

            //delete message
            await DeleteMessage(connectionString, topicPath, subscriptionPath, message, true);
        }
    }
}