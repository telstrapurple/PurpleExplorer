using System.Threading.Tasks;
using MessageBox.Avalonia.Enums;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.Services;
using Splat;

namespace PurpleExplorer.ViewModels
{
    public class MessageDetailsWindowViewModel : ViewModelBase
    {
        private readonly IServiceBusHelper _serviceBusHelper;
        private readonly ILoggingService _loggingService;

        public Message Message { get; set; }
        public ServiceBusSubscription Subscription { get; set; }
        public string ConnectionString { get; set; }

        public MessageDetailsWindowViewModel(IServiceBusHelper serviceBusHelper = null,
            ILoggingService loggingService = null)
        {
            _loggingService = loggingService ?? Locator.Current.GetService<ILoggingService>();
            _serviceBusHelper = serviceBusHelper ?? Locator.Current.GetService<IServiceBusHelper>();
        }

        public async Task ResubmitMessage()
        {
            _loggingService.Log($"Resending DLQ message: {Message.MessageId}");

            await _serviceBusHelper.ResubmitDlqMessage(ConnectionString, Subscription.Topic.Name, Subscription.Name,
                Message);

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

            await _serviceBusHelper.DeadletterMessage(ConnectionString, Subscription.Topic.Name, Subscription.Name,
                Message);

            _loggingService.Log($"Sent message: {Message.MessageId} to dead-letter");
        }
    }
}