using System;
using System.Text;
using AzureMessage = Microsoft.Azure.ServiceBus.Message;

namespace PurpleExplorer.Models;

public class Message
{
    public string MessageId { get; set; }
    public string ContentType { get; set; }
    public string Content { get; set; }
    public string Label { get; set; }
    public long Size { get; set; }
    public string CorrelationId { get; set; }
    public int DeliveryCount { get; set; }
    public long SequenceNumber { get; set; }
    public TimeSpan TimeToLive { get; set; }
    public DateTime EnqueueTimeUtc { get; set; }
    public string DeadLetterReason { get; set; }
    public bool IsDlq { get; }
        
    public Message(AzureMessage azureMessage, bool isDlq)
    {
        this.Content = azureMessage.Body is not null ? Encoding.UTF8.GetString(azureMessage.Body) : string.Empty;
        this.MessageId = azureMessage.MessageId;
        this.CorrelationId = azureMessage.CorrelationId;
        this.DeliveryCount = azureMessage.SystemProperties.DeliveryCount;
        this.ContentType = azureMessage.ContentType;
        this.Label = azureMessage.Label;
        this.SequenceNumber = azureMessage.SystemProperties.SequenceNumber;
        this.Size = azureMessage.Size;
        this.TimeToLive = azureMessage.TimeToLive;
        this.IsDlq = isDlq;
        this.EnqueueTimeUtc = azureMessage.SystemProperties.EnqueuedTimeUtc;
        this.DeadLetterReason = azureMessage.UserProperties.ContainsKey("DeadLetterReason")
            ? azureMessage.UserProperties["DeadLetterReason"].ToString()
            : string.Empty;
    }
}