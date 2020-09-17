using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using PurpleExplorer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        public async Task<IList<ServiceBusSubscription>> GetSubscriptions(string connectionString, string topicPath)
        {
            IList<ServiceBusSubscription> subscriptions = new List<ServiceBusSubscription>();
            ManagementClient client = new ManagementClient(connectionString);

            var topicSubscription = await client.GetSubscriptionsRuntimeInfoAsync(topicPath);
            foreach (var sub in topicSubscription)
            {
                subscriptions.Add(
                    new ServiceBusSubscription
                    {
                        Name = sub.SubscriptionName,
                        MessageCount = sub.MessageCountDetails.ActiveMessageCount,
                        DLQCount = sub.MessageCountDetails.DeadLetterMessageCount,
                    }
                );
            }

            return subscriptions;
        }

        public async Task<IList<Message>> GetMessagesBySubscription(string connectionString, string topicName,
            string subscriptionName)
        {
            var messageReceiver = new MessageReceiver(connectionString,
                EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName), ReceiveMode.PeekLock);
            var subscriptionMessages = await messageReceiver.PeekAsync(_maxMessageCount);

            var result = subscriptionMessages.Select(message => new Message(message, false)).ToList();

            return result;
        }

        public async Task<IList<Message>> GetDlqMessages(string connectionString, string topic, string subscription)
        {
            var path = EntityNameHelper.FormatSubscriptionPath(topic, subscription);
            var deadletterPath = EntityNameHelper.FormatDeadLetterPath(path);
            var receiver = new MessageReceiver(connectionString, deadletterPath, ReceiveMode.PeekLock);
            var receivedMessages = await receiver.PeekAsync(_maxMessageCount);

            var result = receivedMessages.Select(message => new Message(message, true)).ToList();

            return result;
        }

        public async Task<NamespaceInfo> GetNamespaceInfo(string connectionString)
        {
            var client = new ManagementClient(connectionString);
            return await client.GetNamespaceInfoAsync();
        }

        public async Task SendTopicMessage(string connectionString, string topicPath, string message)
        {
            var topicClient = new TopicClient(connectionString, topicPath);
            await topicClient.SendAsync(new AzureMessage() {Body = Encoding.UTF8.GetBytes(message)});
            await topicClient.CloseAsync();
        }

        public async Task DeleteMessage(string connectionString, string topicPath, string subscriptionPath,
            Message message, bool isDlq)
        {
            var path = EntityNameHelper.FormatSubscriptionPath(topicPath, subscriptionPath);
            path = isDlq ? EntityNameHelper.FormatDeadLetterPath(path) : path;
            
            var receiver = new MessageReceiver(connectionString, path, ReceiveMode.PeekLock);

            Task Handler(AzureMessage msg, CancellationToken token)
            {
                if (msg.MessageId.Equals(message.MessageId))
                {
                    receiver.CompleteAsync(msg.SystemProperties.LockToken);
                }

                return Task.CompletedTask;
            }

            Task ExceptionHandler(ExceptionReceivedEventArgs args)
            {
                /*TODO add logging */
                return Task.CompletedTask;
            }

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionHandler)
            {
                MaxConcurrentCalls = 1,    // For simplicity
                AutoComplete = false
            };

            receiver.RegisterMessageHandler(Handler, messageHandlerOptions);
            await receiver.PeekBySequenceNumberAsync(message.SequenceNumber, 1);
        }
    }
}