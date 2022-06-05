using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using PurpleExplorer.Models;
using Message = PurpleExplorer.Models.Message;
using AzureMessage = Microsoft.Azure.ServiceBus.Message;

namespace PurpleExplorer.Helpers
{
    public class QueueHelper : IQueueHelper
    {
        private readonly AppSettings _appSettings;

        public QueueHelper(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task<IList<ServiceBusQueue>> GetQueues(string connectionString)
        {
            IList<ServiceBusQueue> queues = new List<ServiceBusQueue>();
            var client = new ManagementClient(connectionString);
            var queuesInfo = await client.GetQueuesRuntimeInfoAsync(_appSettings.QueueListFetchCount);
            await client.CloseAsync();   
            
            await Task.WhenAll(queuesInfo.Select(async queue =>
            {
                var queueName = queue.Path;

                var newQueue = new ServiceBusQueue(queue)
                {
                    Name = queueName
                };

                queues.Add(newQueue);
            }));

            return queues;
        }
        
        public async Task SendMessage(string connectionString, string queueName, string content)
        {
            var message = new AzureMessage {Body = Encoding.UTF8.GetBytes(content)};
            await SendMessage(connectionString, queueName, message);
        }

        public async Task SendMessage(string connectionString, string queueName, AzureMessage message)
        {
            var client = new QueueClient(connectionString, queueName);
            await client.SendAsync(message);
            await client.CloseAsync();
        }

        public async Task<IList<Message>> GetMessages(string connectionString, string queueName)
        {
            var receiver = new MessageReceiver(connectionString, queueName, ReceiveMode.PeekLock);
            var messages = await receiver.PeekAsync(_appSettings.QueueMessageFetchCount);
            return messages.Select(msg => new Message(msg, false)).ToList();
        }

        public async Task<IList<Message>> GetDlqMessages(string connectionString, string queueName)
        {
            var deadletterPath = EntityNameHelper.FormatDeadLetterPath(queueName);

            var receiver = new MessageReceiver(connectionString, deadletterPath, ReceiveMode.PeekLock);
            var receivedMessages = await receiver.PeekAsync(_appSettings.QueueMessageFetchCount);
            await receiver.CloseAsync();

            return receivedMessages.Select(message => new Message(message, true)).ToList();
        }
        
        public async Task DeadletterMessage(string connectionString, string queue, Message message)
        {
            var receiver = new MessageReceiver(connectionString, queue, ReceiveMode.PeekLock);

            while (true)
            {
                var messages = await receiver.ReceiveAsync(_appSettings.QueueMessageFetchCount);
                if (messages == null || messages.Count == 0)
                {
                    break;
                }

                var foundMessage = messages.FirstOrDefault(m => m.MessageId.Equals(message.MessageId));
                if (foundMessage != null)
                {
                    await receiver.DeadLetterAsync(foundMessage.SystemProperties.LockToken);
                    break;
                }
            }

            await receiver.CloseAsync();
        }
        
        public async Task DeleteMessage(string connectionString, string queue,
            Message message, bool isDlq)
        {
            var path = isDlq ? EntityNameHelper.FormatDeadLetterPath(queue) : queue;

            var receiver = new MessageReceiver(connectionString, path, ReceiveMode.PeekLock);

            while (true)
            {
                var messages = await receiver.ReceiveAsync(_appSettings.QueueMessageFetchCount);
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
        
        private async Task<AzureMessage> PeekDlqMessageBySequenceNumber(string connectionString, string queue, long sequenceNumber)
        {
            var deadletterPath = EntityNameHelper.FormatDeadLetterPath(queue);

            var receiver = new MessageReceiver(connectionString, deadletterPath, ReceiveMode.PeekLock);
            var azureMessage = await receiver.PeekBySequenceNumberAsync(sequenceNumber);
            await receiver.CloseAsync();
            
            return azureMessage;
        }
        
        public async Task ResubmitDlqMessage(string connectionString, string queue, Message message)
        {
            var azureMessage = await PeekDlqMessageBySequenceNumber(connectionString, queue, message.SequenceNumber);
            var clonedMessage = azureMessage.CloneMessage();

            await SendMessage(connectionString, queue, clonedMessage);

            await DeleteMessage(connectionString, queue, message, true);
        }

        public async Task<long> PurgeMessages(string connectionString, string queue, bool isDlq)
        {
            var path = isDlq ? EntityNameHelper.FormatDeadLetterPath(queue) : queue;

            long purgedCount = 0;
            var receiver = new MessageReceiver(connectionString, path, ReceiveMode.ReceiveAndDelete);
            var operationTimeout = TimeSpan.FromSeconds(5);
            while (true)
            {
                var messages = await receiver.ReceiveAsync(_appSettings.QueueMessageFetchCount, operationTimeout);
                if (messages == null || messages.Count == 0)
                {
                    break;
                }

                purgedCount += messages.Count;
            }

            await receiver.CloseAsync();
            return purgedCount;
        }
        
        public async Task<long> TransferDlqMessages(string connectionString, string queuePath)
        {
            var path = EntityNameHelper.FormatDeadLetterPath(queuePath);

            long transferredCount = 0;
            MessageReceiver receiver = null;
            QueueClient sender = null;
            try
            {
                receiver = new MessageReceiver(connectionString, path, ReceiveMode.ReceiveAndDelete);
                sender = new QueueClient(connectionString, queuePath);
                var operationTimeout = TimeSpan.FromSeconds(5);
                while (true)
                {
                    var messages = await receiver.ReceiveAsync(_appSettings.QueueMessageFetchCount, operationTimeout);
                    if (messages == null || messages.Count == 0)
                    {
                        break;
                    }

                    await sender.SendAsync(messages);

                    transferredCount += messages.Count;
                }
            }
            finally
            {
                if (receiver != null) 
                    await receiver.CloseAsync();

                if (sender != null)
                    await sender.CloseAsync();
            }

            return transferredCount;
        }
    }

    public interface IQueueHelper
    {
        Task<IList<ServiceBusQueue>> GetQueues(string connectionString);
        public Task SendMessage(string connectionString, string topicPath, string content);
        public Task SendMessage(string connectionString, string topicPath, AzureMessage message);
        Task<IList<Message>> GetMessages(string connectionString, string queueName);
        Task<IList<Message>> GetDlqMessages(string connectionString, string queueName);
        Task DeadletterMessage(string connectionString, string queue, Message message);
        Task DeleteMessage(string connectionString, string queue,
            Message message, bool isDlq);
        Task ResubmitDlqMessage(string connectionString, string queue, Message message);
        Task<long> PurgeMessages(string connectionString, string queue, bool isDlq);
        Task<long> TransferDlqMessages(string connectionString, string queue);
    }
}