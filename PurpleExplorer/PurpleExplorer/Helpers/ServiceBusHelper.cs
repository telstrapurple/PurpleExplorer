using Microsoft.Azure.ServiceBus.Management;
using PurpleExplorer.Models;
using System;
using System.Collections.Generic;
using System.Text;
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

                var namespaceInfo = _client.GetNamespaceInfoAsync().Result;
                var busTopics = await _client.GetTopicsAsync();

                foreach (var obj in busTopics)
                {
                    topics.Add(new ServiceBusTopic()
                    {
                        Name = obj.Path
                    });
                }

            }

            catch (Exception ex)
            {
                throw ex;
                // Logging here.
            }

            return topics;
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
