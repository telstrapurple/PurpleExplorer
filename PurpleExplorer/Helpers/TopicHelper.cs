using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using PurpleExplorer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using Message = PurpleExplorer.Models.Message;
using AzureMessage = Microsoft.Azure.ServiceBus.Message;

namespace PurpleExplorer.Helpers;

public class TopicHelper : BaseHelper, ITopicHelper
{
    private const int MaxTopicListFetchCountPerPage = 100;
    private readonly AppSettings _appSettings;

    public TopicHelper(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public async Task<IList<ServiceBusTopic>> GetTopicsAndSubscriptions(ServiceBusConnectionString connectionString)
    {
        var client = GetManagementClient(connectionString);
        var topics = await GetTopicsWithSubscriptions(client);
        await client.CloseAsync();
        return topics;
    }

    private async Task<ServiceBusTopic> CreateTopicWithSubscriptions(ManagementClient client, TopicDescription topicDescription)
    {
        var topic = new ServiceBusTopic(topicDescription);
        var subscriptions = await GetSubscriptions(client, topicDescription.Path);
        topic.AddSubscriptions(subscriptions.ToArray());
        return topic;
    }

    private async Task<List<ServiceBusTopic>> GetTopicsWithSubscriptions(ManagementClient client)
    {
        var topicDescriptions = new List<TopicDescription>();
        var numberOfPages = _appSettings.TopicListFetchCount / MaxTopicListFetchCountPerPage;
        var remainder = _appSettings.TopicListFetchCount % (numberOfPages * MaxTopicListFetchCountPerPage);

        for (int pageCount = 0; pageCount < numberOfPages; pageCount++)
        {
            var numberToSkip = MaxTopicListFetchCountPerPage * pageCount;
            var page = await client.GetTopicsAsync(MaxTopicListFetchCountPerPage, numberToSkip);
            if (page.Any())
            {
                topicDescriptions.AddRange(page);
            }
            else
            {
                return (await Task.WhenAll(topicDescriptions
                    .Select(async topic => await CreateTopicWithSubscriptions(client, topic)))).ToList();
            }
        }

        if (remainder > 0)
        {
            var numberAlreadyFetched = numberOfPages > 0
                ? MaxTopicListFetchCountPerPage * numberOfPages
                : 0;
            var remainingItems = await client.GetTopicsAsync(
                remainder,
                numberAlreadyFetched);
            topicDescriptions.AddRange(remainingItems);
        }

        var topics = await Task.WhenAll(topicDescriptions
            .Select(async topic => await CreateTopicWithSubscriptions(client, topic)));
        return topics.ToList();
    }

    public async Task<ServiceBusTopic> GetTopic(ServiceBusConnectionString connectionString, string topicPath,
        bool retrieveSubscriptions)
    {
        var client = GetManagementClient(connectionString);
        var busTopics = await client.GetTopicAsync(topicPath);
        var newTopic = new ServiceBusTopic(busTopics);

        if (retrieveSubscriptions)
        {
            var subscriptions = await GetSubscriptions(client, newTopic.Name);
            newTopic.AddSubscriptions(subscriptions.ToArray());
        }
        
        await client.CloseAsync();
        return newTopic;
    }

    public async Task<ServiceBusSubscription> GetSubscription(ServiceBusConnectionString connectionString,
        string topicPath, string subscriptionName)
    {
        var client = GetManagementClient(connectionString);
        var runtimeInfo = await client.GetSubscriptionRuntimeInfoAsync(topicPath, subscriptionName);
        await client.CloseAsync();

        return new ServiceBusSubscription(runtimeInfo);
    }

    private async Task<IList<ServiceBusSubscription>> GetSubscriptions(
        ManagementClient client,
        string topicPath)
    {
        IList<ServiceBusSubscription> subscriptions = new List<ServiceBusSubscription>();
        var topicSubscription = await client.GetSubscriptionsRuntimeInfoAsync(topicPath);

        foreach (var sub in topicSubscription)
        {
            subscriptions.Add(new ServiceBusSubscription(sub));
        }

        return subscriptions;
    }

    public async Task<IList<Message>> GetMessagesBySubscription(ServiceBusConnectionString connectionString,
        string topicName,
        string subscriptionName)
    {
        var path = EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName);

        var messageReceiver = GetMessageReceiver(connectionString, path, ReceiveMode.PeekLock);
        var subscriptionMessages = await messageReceiver.PeekAsync(_appSettings.TopicMessageFetchCount);
        await messageReceiver.CloseAsync();

        var result = subscriptionMessages.Select(message => new Message(message, false)).ToList();
        return result;
    }

    public async Task<IList<Message>> GetDlqMessages(ServiceBusConnectionString connectionString, string topic,
        string subscription)
    {
        var path = EntityNameHelper.FormatSubscriptionPath(topic, subscription);
        var deadletterPath = EntityNameHelper.FormatDeadLetterPath(path);

        var receiver = GetMessageReceiver(connectionString, deadletterPath, ReceiveMode.PeekLock);
        var receivedMessages = await receiver.PeekAsync(_appSettings.TopicMessageFetchCount);
        await receiver.CloseAsync();

        var result = receivedMessages.Select(message => new Message(message, true)).ToList();
        return result;
    }

    public async Task<NamespaceInfo> GetNamespaceInfo(ServiceBusConnectionString connectionString)
    {
        var client = GetManagementClient(connectionString);
        return await client.GetNamespaceInfoAsync();
    }

    public async Task SendMessage(ServiceBusConnectionString connectionString, string topicPath, string content)
    {
        var message = new AzureMessage { Body = Encoding.UTF8.GetBytes(content) };
        await SendMessage(connectionString, topicPath, message);
    }

    public async Task SendMessage(ServiceBusConnectionString connectionString, string topicPath, AzureMessage message)
    {
        var topicClient = GetTopicClient(connectionString, topicPath);
        await topicClient.SendAsync(message);
        await topicClient.CloseAsync();
    }

    public async Task DeleteMessage(ServiceBusConnectionString connectionString, string topicPath,
        string subscriptionPath,
        Message message, bool isDlq)
    {
        var path = EntityNameHelper.FormatSubscriptionPath(topicPath, subscriptionPath);
        path = isDlq ? EntityNameHelper.FormatDeadLetterPath(path) : path;

        var receiver = GetMessageReceiver(connectionString, path, ReceiveMode.PeekLock);

        while (true)
        {
            var messages = await receiver.ReceiveAsync(_appSettings.TopicMessageFetchCount);
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

    public async Task<long> PurgeMessages(ServiceBusConnectionString connectionString, string topicPath,
        string subscriptionPath,
        bool isDlq)
    {
        var path = EntityNameHelper.FormatSubscriptionPath(topicPath, subscriptionPath);
        path = isDlq ? EntityNameHelper.FormatDeadLetterPath(path) : path;

        long purgedCount = 0;
        var receiver = GetMessageReceiver(connectionString, path, ReceiveMode.ReceiveAndDelete);
        var operationTimeout = TimeSpan.FromSeconds(5);
        while (true)
        {
            var messages = await receiver.ReceiveAsync(_appSettings.TopicMessageFetchCount, operationTimeout);
            if (messages == null || messages.Count == 0)
            {
                break;
            }

            purgedCount += messages.Count;
        }

        await receiver.CloseAsync();
        return purgedCount;
    }

    public async Task<long> TransferDlqMessages(ServiceBusConnectionString connectionString, string topicPath,
        string subscriptionPath)
    {
        var path = EntityNameHelper.FormatSubscriptionPath(topicPath, subscriptionPath);
        path = EntityNameHelper.FormatDeadLetterPath(path);

        long transferredCount = 0;
        MessageReceiver receiver = null;
        TopicClient sender = null;
        try
        {
            receiver = GetMessageReceiver(connectionString, path, ReceiveMode.ReceiveAndDelete);
            sender = GetTopicClient(connectionString, topicPath);
            var operationTimeout = TimeSpan.FromSeconds(5);
            while (true)
            {
                var messages = await receiver.ReceiveAsync(_appSettings.TopicMessageFetchCount, operationTimeout);
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

    private async Task<AzureMessage> PeekDlqMessageBySequenceNumber(ServiceBusConnectionString connectionString,
        string topicPath,
        string subscriptionPath, long sequenceNumber)
    {
        var path = EntityNameHelper.FormatSubscriptionPath(topicPath, subscriptionPath);
        var deadletterPath = EntityNameHelper.FormatDeadLetterPath(path);

        var receiver = GetMessageReceiver(connectionString, deadletterPath, ReceiveMode.PeekLock);
        var azureMessage = await receiver.PeekBySequenceNumberAsync(sequenceNumber);
        await receiver.CloseAsync();

        return azureMessage;
    }

    public async Task ResubmitDlqMessage(ServiceBusConnectionString connectionString, string topicPath,
        string subscriptionPath,
        Message message)
    {
        var azureMessage = await PeekDlqMessageBySequenceNumber(connectionString, topicPath, subscriptionPath,
            message.SequenceNumber);
        var clonedMessage = azureMessage.CloneMessage();

        await SendMessage(connectionString, topicPath, clonedMessage);

        await DeleteMessage(connectionString, topicPath, subscriptionPath, message, true);
    }

    public async Task DeadletterMessage(ServiceBusConnectionString connectionString, string topicPath,
        string subscriptionPath,
        Message message)
    {
        var path = EntityNameHelper.FormatSubscriptionPath(topicPath, subscriptionPath);

        var receiver = GetMessageReceiver(connectionString, path, ReceiveMode.PeekLock);

        while (true)
        {
            var messages = await receiver.ReceiveAsync(_appSettings.TopicMessageFetchCount);
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
}