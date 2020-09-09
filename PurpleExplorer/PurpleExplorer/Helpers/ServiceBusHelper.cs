using Microsoft.Azure.ServiceBus.Management;
using PurpleExplorer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PurpleExplorer.Helpers
{
    public class ServiceBusHelper
    {
        private string _connectionString;
        private ManagementClient _client;

        public ServiceBusHelper(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public async Task<IList<ServiceBusTopic>> GetTopics()
        {
            IList<ServiceBusTopic> topics = new List<ServiceBusTopic>();

            try
            {
                if (_client == null)
                {
                    _client = new ManagementClient(_connectionString);
                }
                
                var busTopics = await _client.GetTopicsAsync();
                foreach (var topic in busTopics)
                {
                    var topicName = topic.Path;
                    var subscriptions = await GetSubscriptions(topicName);
                    
                    ServiceBusTopic newTopic = new ServiceBusTopic(subscriptions)
                    {
                        Name = topicName
                    };
                    topics.Add(newTopic);
                }
                // TODO Task.WhenAll(topics.Select(t => helper.GetSubscriptions(t.Name))) ??
            }

            catch (Exception ex)
            {
                throw ex;
                // Logging here.
            }

            return topics;
        }

        public async Task<IList<ServiceBusSubscription>> GetSubscriptions(string topicPath)
        {
            IList<ServiceBusSubscription> subscriptions = new List<ServiceBusSubscription>();
            //TODO: check if it is fine to use private classfield instantiated in constructor - need to handle exception in case connection string is wrong

            try
            {
                if (_client == null)
                {
                    _client = new ManagementClient(_connectionString);
                }

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
            catch (Exception e)
            {
                //TODO.  Add error handling.
            }

            return subscriptions;
        }

        public async Task<NamespaceInfo> GetNamespaceInfo()
        {
            try
            {
                if (_client == null)
                {
                    _client = new ManagementClient(_connectionString);
                }
                
                return await _client.GetNamespaceInfoAsync();
            }

            catch (Exception ex)
            {
                //TODO.  Add error handling.
            }

            return null;
        }
    }
}