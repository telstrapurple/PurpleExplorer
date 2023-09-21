using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia.Enums;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.Services;
using Splat;

namespace PurpleExplorer.ViewModels;

public class MessageDetailsWindowViewModel : ViewModelBase
{
    private readonly ITopicHelper _topicHelper;
    private readonly IQueueHelper _queueHelper;
    private readonly ILoggingService _loggingService;

    public Message Message { get; set; }
    public ServiceBusSubscription Subscription { get; set; }
    public ServiceBusQueue Queue { get; set; }
    public ServiceBusConnectionString ConnectionString { get; set; }

    public MessageDetailsWindowViewModel(ITopicHelper topicHelper = null,
        ILoggingService loggingService = null,
        IQueueHelper queueHelper = null)
    {
        _loggingService = loggingService ?? Locator.Current.GetService<ILoggingService>();
        _topicHelper = topicHelper ?? Locator.Current.GetService<ITopicHelper>();
        _queueHelper = queueHelper ?? Locator.Current.GetService<IQueueHelper>();
    }

    public async void DeleteMessage(Window window)
    {
        _loggingService.Log("DANGER NOTE: Deleting requires receiving all the messages up to the selected message to perform this action and this increases the DeliveryCount of the messages");
        string deletingPath = null;
        if (Subscription != null)
        {
            deletingPath = Message.IsDlq
                ? $"{Subscription.Topic.Name}/{Subscription.Name}/$DeadLetterQueue"
                : $"{Subscription.Topic.Name}/{Subscription.Name}";
        }

        if (Queue != null)
        {
            deletingPath = Message.IsDlq
                ? $"{Queue.Name}/$DeadLetterQueue"
                : $"{Queue.Name}";
        }
            
        var buttonResult = await MessageBoxHelper.ShowConfirmation(
            $"Deleting message from {deletingPath}",
            $"DANGER!!! READ CAREFULLY \n" +
            $"Deleting requires receiving all the messages up to the selected message to perform this action and this increases the DeliveryCount of the messages. \n" +
            $"There can be consequences to other messages in this subscription, Are you sure? \n \n" +
            $"Are you sure you would like to delete the message with ID: {Message.MessageId} AND increase the delivery count of ALL the messages before it?");

        // Because buttonResult can be None or No
        if (buttonResult != ButtonResult.Yes)
        {
            return;
        }

        _loggingService.Log($"User accepted to receive messages in order to delete message {Message.MessageId}. This is going to increases the DeliveryCount of the messages before it.");
        _loggingService.Log($"Deleting message {Message.MessageId}... (might take some seconds)");

        if (Subscription != null)
        {
            var connectionString = Subscription.Topic.ServiceBus.ConnectionString;
            await _topicHelper.DeleteMessage(connectionString, Subscription.Topic.Name, Subscription.Name,
                Message, Message.IsDlq);
                
            if(!Message.IsDlq) 
                Subscription.RemoveMessage(Message.MessageId);
            else
                Subscription.RemoveDlqMessage(Message.MessageId);
        }

        if (Queue != null)
        {
            var connectionString = Queue.ServiceBus.ConnectionString;
            await _queueHelper.DeleteMessage(connectionString, Queue.Name, Message, Message.IsDlq);
                
            if(!Message.IsDlq) 
                Queue.RemoveMessage(Message.MessageId);
            else
                Queue.RemoveDlqMessage(Message.MessageId);
        }

        _loggingService.Log($"Message deleted, MessageId: {Message.MessageId}");
        window.Close();
    }

    public async Task ResubmitMessage()
    {
        _loggingService.Log($"Resending DLQ message: {Message.MessageId}");

        if (Subscription != null)
        {
            await _topicHelper.ResubmitDlqMessage(ConnectionString, Subscription.Topic.Name, Subscription.Name,
                Message);
        }

        if (Queue != null)
        {
            await _queueHelper.ResubmitDlqMessage(ConnectionString, Queue.Name, Message);
        }

        _loggingService.Log($"Resent DLQ message: {Message.MessageId}");
    }
        
    public async Task DeadletterMessage()
    {
        _loggingService.Log("DANGER NOTE: Sending to dead-letter requires receiving all the messages up to the selected message to perform this action and this increases the DeliveryCount of the messages");
        var buttonResult = await MessageBoxHelper.ShowConfirmation(
            $"Sending message to dead-letter",
            $"DANGER!!! READ CAREFULLY \n" +
            $"Sending to dead-letter requires receiving all the messages up to the selected message to perform this action and this increases the DeliveryCount of the messages. \n" +
            $"There can be consequences to other messages in this subscription, Are you sure? \n \n" +
            $"Are you sure you would like to send the message {Message.MessageId} AND increase the delivery count of ALL the messages before it?");

        // Because buttonResult can be None or No
        if (buttonResult != ButtonResult.Yes)
        {
            return;
        }
            
        _loggingService.Log($"User accepted to receive messages in order to send message {Message.MessageId} to dead-letter. This is going to increases the DeliveryCount of the messages before it.");
        _loggingService.Log($"Sending message: {Message.MessageId} to dead-letter");

        if (Subscription != null)
        {
            await _topicHelper.DeadletterMessage(ConnectionString, Subscription.Topic.Name, Subscription.Name,
                Message);
        }

        if (Queue != null)
        {
            await _queueHelper.DeadletterMessage(ConnectionString, Queue.Name, Message);
        }

        _loggingService.Log($"Sent message: {Message.MessageId} to dead-letter");
    }
}