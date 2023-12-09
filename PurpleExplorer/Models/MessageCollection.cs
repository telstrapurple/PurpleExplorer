using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using ReactiveUI;

namespace PurpleExplorer.Models;

/// <summary>
/// Represents either a subscription or a queue
/// </summary>
public abstract class MessageCollection : ReactiveObject
{
    // These are needed to be set before fetching messages, in the second constructor
    private long _messageCount;
    private long _totalMessageCount;
    private long _dlqCount;
    private long _totalDlqCount;
       
    public ObservableCollection<Message> Messages { get; }
    public ObservableCollection<Message> DlqMessages { get; }
        
    public long MessageCount
    {
        get => _messageCount;
        private set => this.RaiseAndSetIfChanged(ref _messageCount, value);
    }

    public long TotalMessageCount
    {
        get => _totalMessageCount;
        private set => this.RaiseAndSetIfChanged(ref _totalMessageCount, value);
    }

    public long DlqCount
    {
        get => _dlqCount;
        private set => this.RaiseAndSetIfChanged(ref _dlqCount, value);
    }

    public long TotalDlqCount
    {
        get => _totalDlqCount;
        private set => this.RaiseAndSetIfChanged(ref _totalDlqCount, value);
    }

    public long TotalTotalMessageCount => _totalMessageCount + _totalDlqCount;

    protected MessageCollection()
    {
        Messages = new ObservableCollection<Message>();
        DlqMessages = new ObservableCollection<Message>();
    }

    protected MessageCollection(long totalMessageCount, long dlqCount) : this()
    {
        _totalMessageCount = totalMessageCount;
        _totalDlqCount = dlqCount;
    }

    public void AddMessages(IEnumerable<Message> messages)
    {
        Messages.AddRange(messages);
        MessageCount = Messages.Count;
    }

    public void RemoveMessage(string messageId)
    {
        Messages.Remove(Messages.Single(msg => msg.MessageId.Equals(messageId)));
        MessageCount = Messages.Count;
    }

    public void SetTotalMessageCount(long n)
    {
        TotalMessageCount = n;
    }
        
    public void ClearMessages()
    {
        Messages.Clear();
        MessageCount = Messages.Count;
    }
        
    public void AddDlqMessages(IEnumerable<Message> dlqMessages)
    {
        DlqMessages.AddRange(dlqMessages);
        DlqCount = DlqMessages.Count;
    }
        
    public void RemoveDlqMessage(string messageId)
    {
        DlqMessages.Remove(DlqMessages.Single(msg => msg.MessageId.Equals(messageId)));
        DlqCount = DlqMessages.Count;
    }

    public void SetTotalDlqMessageCount(long n)
    {
        TotalDlqCount = n;
    }
        
    public void ClearDlqMessages()
    {
        DlqMessages.Clear();
        DlqCount = DlqMessages.Count;
    }
}