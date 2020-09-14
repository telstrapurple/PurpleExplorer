using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using PurpleExplorer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurpleExplorer.Helpers
{
    public class ServiceBusHelper : IServiceBusHelper
    {
        public async Task<IList<ServiceBusTopic>> GetTopics(string connectionString)
        {
            IList<ServiceBusTopic> topics = new List<ServiceBusTopic>();
            ManagementClient client = new ManagementClient(connectionString);

            try
            {
                var busTopics = await client.GetTopicsAsync();

                await Task.WhenAll(busTopics.Select(async t =>
                {
                    var topicName = t.Path;
                    var subscriptions = await GetSubscriptions(connectionString, topicName);

                    ServiceBusTopic newTopic = new ServiceBusTopic()
                    {
                        Name = topicName, 
                        Subscriptions = new System.Collections.ObjectModel.ObservableCollection<ServiceBusSubscription>(subscriptions)
                    };

                    foreach (var sub in newTopic.Subscriptions)
                    {
                        sub.Topic = newTopic;
                    }

                    topics.Add(newTopic);
                }));
            }

            catch (Exception)
            {
                // Logging here.
            }

            return topics;
        }

        public async Task<IList<ServiceBusSubscription>> GetSubscriptions(string connectionString, string topicPath)
        {
            IList<ServiceBusSubscription> subscriptions = new List<ServiceBusSubscription>();
            ManagementClient client = new ManagementClient(connectionString);

            try
            {
                var topicSubscription = await client.GetSubscriptionsRuntimeInfoAsync(topicPath);
                foreach (var sub in topicSubscription)
                {
                    subscriptions.Add(
                        new ServiceBusSubscription()
                        {
                            Name = sub.SubscriptionName,
                            MessageCount = sub.MessageCountDetails.ActiveMessageCount,
                            DLQCount = sub.MessageCountDetails.DeadLetterMessageCount,
                        }
                    );
                }
            }
            catch (Exception)
            {
                //TODO.  Add error handling.
            }

            return subscriptions;
        }

        public async Task<IList<Models.Message>> GetMessagesBySubscription(string connectionString, string topicName, string subscriptionName)
        {
            var messageReceiver = new MessageReceiver(connectionString, EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName), ReceiveMode.PeekLock);
            var subscriptionMessages = await messageReceiver.PeekAsync(100);

            var messageList = (from o in subscriptionMessages select new Models.Message() { Content = Encoding.UTF8.GetString(o.Body), Size = o.Size }).ToList();
            return messageList;
        }
        public async Task<NamespaceInfo> GetNamespaceInfo(string connectionString)
        {
            ManagementClient client = new ManagementClient(connectionString);
            return await client.GetNamespaceInfoAsync();
        }

    }
}