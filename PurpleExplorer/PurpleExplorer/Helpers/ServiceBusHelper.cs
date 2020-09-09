using Microsoft.Azure.ServiceBus.Management;
using PurpleExplorer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurpleExplorer.Helpers
{
    public class ServiceBusHelper
    {
        private readonly ManagementClient _client;

        public ServiceBusHelper(string connectionString)
        {
            _client = new ManagementClient(connectionString);
        }

        public async Task<IList<ServiceBusTopic>> GetTopics()
        {
            IList<ServiceBusTopic> topics = new List<ServiceBusTopic>();

            try
            {
                var busTopics = await _client.GetTopicsAsync();

                await Task.WhenAll(busTopics.Select(async t =>
                {
                    var topicName = t.Path;
                    var subscriptions = await GetSubscriptions(topicName);

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

        public async Task<IList<ServiceBusSubscription>> GetSubscriptions(string topicPath)
        {
            IList<ServiceBusSubscription> subscriptions = new List<ServiceBusSubscription>();
            try
            {
                var topicSubscription = await _client.GetSubscriptionsRuntimeInfoAsync(topicPath);
                foreach (var sub in topicSubscription)
                {
                    subscriptions.Add(
                        new ServiceBusSubscription()
                        {
                            Name = sub.SubscriptionName,
                            MessageCount = sub.MessageCount
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

        public async Task<NamespaceInfo> GetNamespaceInfo()
        {
            return await _client.GetNamespaceInfoAsync();
        }
    }
}