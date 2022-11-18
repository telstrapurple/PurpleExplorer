using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.ServiceBus.Primitives;
using PurpleExplorer.Models;

namespace PurpleExplorer.Helpers
{
	public class BaseHelper
	{
		public BaseHelper()
		{
		}

        protected ManagementClient GetManagementClient(ServiceBusConnectionString connectionString)
        {
            if (connectionString.UseManagedIdentity)
            {
                var tokenProvider = GetTokenProvider(connectionString);
                return new ManagementClient(connectionString.ConnectionString, tokenProvider);
            }
            else
            {
                return new ManagementClient(connectionString.ConnectionString);
            }
        }

        protected TokenProvider GetTokenProvider(ServiceBusConnectionString connectionString)
        {
            if (connectionString.UseManagedIdentity)
            {
                return TokenProvider.CreateManagedIdentityTokenProvider();
            }
            else
            {
                throw new NotImplementedException("Unknown token provider.");
            }
        }

        protected MessageReceiver GetMessageReceiver(ServiceBusConnectionString connectionString, string path, ReceiveMode receiveMode)
        {
            return connectionString.UseManagedIdentity
                ? new MessageReceiver(connectionString.ConnectionString, path, GetTokenProvider(connectionString), receiveMode: receiveMode)
                : new MessageReceiver(connectionString.ConnectionString, path, receiveMode);
        }

        protected TopicClient GetTopicClient(ServiceBusConnectionString connectionString, string path)
        {
            return connectionString.UseManagedIdentity
                ? new TopicClient(connectionString.ConnectionString, path, GetTokenProvider(connectionString))
                : new TopicClient(connectionString.ConnectionString, path);
        }

        protected QueueClient GetQueueClient(ServiceBusConnectionString connectionString, string queueName)
        {
            return connectionString.UseManagedIdentity
                ? new QueueClient(connectionString.ConnectionString, queueName, GetTokenProvider(connectionString))
                : new QueueClient(connectionString.ConnectionString, queueName);
        }
    }
}

