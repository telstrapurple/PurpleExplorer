using Microsoft.Azure.ServiceBus.Management;
using PurpleExplorer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurpleExplorer.Helpers
{
    public class ServiceBusHelper : IServiceBusHelper
    {
        private ManagementClient _client;

        public async Task<IList<ServiceBusTopic>> GetTopics(string connectionString)
        {
            IList<ServiceBusTopic> topics = new List<ServiceBusTopic>();

            try
            {
                if (_client == null)
                {
                    _client = new ManagementClient(connectionString);
                }

                var busTopics = await _client.GetTopicsAsync();

                await Task.WhenAll(busTopics.Select(async t =>
                {
                    var topicName = t.Path;
                    var subscriptions = await GetSubscriptions(connectionString, topicName);

                    ServiceBusTopic newTopic = new ServiceBusTopic(subscriptions)
                    {
                        Name = topicName
                    };
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
            try
            {
                if (_client == null)
                {
                    _client = new ManagementClient(connectionString);
                }

                var topicSubscription = await _client.GetSubscriptionsRuntimeInfoAsync(topicPath);
                foreach (var sub in topicSubscription)
                {
                    subscriptions.Add(
                        new ServiceBusSubscription()
                        {
                            Name = sub.SubscriptionName,
                            MessageCount = sub.MessageCountDetails.ActiveMessageCount,
                            DLQCount = sub.MessageCountDetails.DeadLetterMessageCount
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

        public async Task<NamespaceInfo> GetNamespaceInfo(string connectionString)
        {
            if (_client == null)
            {
                _client = new ManagementClient(connectionString);
            }

            return await _client.GetNamespaceInfoAsync();
        }
    }
}