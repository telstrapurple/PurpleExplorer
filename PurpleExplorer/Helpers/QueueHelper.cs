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

namespace PurpleExplorer.Helpers;

public class QueueHelper : BaseHelper, IQueueHelper
{
    private readonly AppSettings _appSettings;

    public QueueHelper(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public async Task<IList<ServiceBusQueue>> GetQueues(ServiceBusConnectionString connectionString)
    {
        var client = GetManagementClient(connectionString);
        var queues = await GetQueues(client);
        await client.CloseAsync();
        return queues;
    }

    public async Task SendMessage(ServiceBusConnectionString connectionString, string queueName, string content)
    {
        var message = new AzureMessage { Body = Encoding.UTF8.GetBytes(content) };
        await SendMessage(connectionString, queueName, message);
    }

    public async Task SendMessage(ServiceBusConnectionString connectionString, string queueName, AzureMessage message)
    {
        var client = GetQueueClient(connectionString, queueName);
        await client.SendAsync(message);
        await client.CloseAsync();
    }

    public async Task<IList<Message>> GetMessages(ServiceBusConnectionString connectionString, string queueName)
    {
        var receiver = GetMessageReceiver(connectionString, queueName, ReceiveMode.PeekLock);
        var messages = await receiver.PeekAsync(_appSettings.QueueMessageFetchCount);
        return messages.Select(msg => new Message(msg, false)).ToList();
    }

    public async Task<IList<Message>> GetDlqMessages(ServiceBusConnectionString connectionString, string queueName)
    {
        var deadletterPath = EntityNameHelper.FormatDeadLetterPath(queueName);

        var receiver = GetMessageReceiver(connectionString, deadletterPath, ReceiveMode.PeekLock);
        var receivedMessages = await receiver.PeekAsync(_appSettings.QueueMessageFetchCount);
        await receiver.CloseAsync();

        return receivedMessages.Select(message => new Message(message, true)).ToList();
    }

    public async Task DeadletterMessage(ServiceBusConnectionString connectionString, string queue, Message message)
    {
        var receiver = GetMessageReceiver(connectionString, queue, ReceiveMode.PeekLock);

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

    public async Task DeleteMessage(ServiceBusConnectionString connectionString, string queue,
        Message message, bool isDlq)
    {
        var path = isDlq ? EntityNameHelper.FormatDeadLetterPath(queue) : queue;

        var receiver = GetMessageReceiver(connectionString, path, ReceiveMode.PeekLock);

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

    private async Task<AzureMessage> PeekDlqMessageBySequenceNumber(ServiceBusConnectionString connectionString,
        string queue, long sequenceNumber)
    {
        var deadletterPath = EntityNameHelper.FormatDeadLetterPath(queue);

        var receiver = GetMessageReceiver(connectionString, deadletterPath, ReceiveMode.PeekLock);
        var azureMessage = await receiver.PeekBySequenceNumberAsync(sequenceNumber);
        await receiver.CloseAsync();

        return azureMessage;
    }

    public async Task ResubmitDlqMessage(ServiceBusConnectionString connectionString, string queue, Message message)
    {
        var azureMessage = await PeekDlqMessageBySequenceNumber(connectionString, queue, message.SequenceNumber);
        var clonedMessage = azureMessage.CloneMessage();

        await SendMessage(connectionString, queue, clonedMessage);

        await DeleteMessage(connectionString, queue, message, true);
    }

    public async Task<long> PurgeMessages(ServiceBusConnectionString connectionString, string queue, bool isDlq)
    {
        var path = isDlq ? EntityNameHelper.FormatDeadLetterPath(queue) : queue;

        long purgedCount = 0;

        var receiver = GetMessageReceiver(connectionString, path, ReceiveMode.ReceiveAndDelete);
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

    public async Task<long> TransferDlqMessages(ServiceBusConnectionString connectionString, string queuePath)
    {
        var path = EntityNameHelper.FormatDeadLetterPath(queuePath);

        long transferredCount = 0;
        MessageReceiver receiver = null;
        QueueClient sender = null;
        try
        {
            receiver = GetMessageReceiver(connectionString, path, ReceiveMode.ReceiveAndDelete);
            sender = GetQueueClient(connectionString, queuePath);
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
    
    private async Task<List<ServiceBusQueue>> GetQueues(ManagementClient client)
    {
        var queueInfos = new List<QueueRuntimeInfo>();
        var numberOfPages = _appSettings.QueueListFetchCount / MaxRequestItemsPerPage;
        var remainder = _appSettings.QueueListFetchCount % (numberOfPages * MaxRequestItemsPerPage);

        for (int pageCount = 0; pageCount < numberOfPages; pageCount++)
        {
            var numberToSkip = MaxRequestItemsPerPage * pageCount;
            var page = await client.GetQueuesRuntimeInfoAsync(MaxRequestItemsPerPage, numberToSkip);
            if (page.Any())
            {
                queueInfos.AddRange(page);
            }
            else
            {
                return queueInfos
                    .Select(q => new ServiceBusQueue(q)
                    {
                        Name = q.Path
                    }).ToList();
            }
        }

        if (remainder > 0)
        {
            var numberAlreadyFetched = numberOfPages > 0
                ? MaxRequestItemsPerPage * numberOfPages
                : 0;
            var remainingItems = await client.GetQueuesRuntimeInfoAsync(
                remainder,
                numberAlreadyFetched);
            queueInfos.AddRange(remainingItems);
        }

        return queueInfos.Select(q => new ServiceBusQueue(q)
        {
            Name = q.Path
        }).ToList();
    }
}