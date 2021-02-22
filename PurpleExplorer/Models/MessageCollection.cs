using System.Collections.Generic;
using System.Collections.ObjectModel;
using DynamicData;
using ReactiveUI;

namespace PurpleExplorer.Models
{
    /// <summary>
    /// Represents either a subscription or a queue
    /// </summary>
    public abstract class MessageCollection : ReactiveObject
    {
        private long _messageCount;
        private long _dlqCount;
       
        public ObservableCollection<Message> Messages { get; }
        public ObservableCollection<Message> DlqMessages { get; }
        
        public long MessageCount
        {
            get => _messageCount;
            set => this.RaiseAndSetIfChanged(ref _messageCount, value);
        }
        
        public long DlqCount
        {
            get => _dlqCount;
            set => this.RaiseAndSetIfChanged(ref _dlqCount, value);
        }

        protected MessageCollection()
        {
            Messages = new ObservableCollection<Message>();
            DlqMessages = new ObservableCollection<Message>();
        }

        protected MessageCollection(long messageCount, long dlqCount) : this()
        {
            _messageCount = messageCount;
            _dlqCount = dlqCount;
        }

        public void AddMessages(IEnumerable<Message> messages)
        {
            Messages.AddRange(messages);
            _messageCount = Messages.Count;
        }

        public void ClearMessages()
        {
            Messages.Clear();
            _messageCount = 0;
        }
        
        public void AddDlqMessages(IEnumerable<Message> dlqMessages)
        {
            DlqMessages.AddRange(dlqMessages);
            _dlqCount = DlqMessages.Count;
        }
        
        public void ClearDlqMessages()
        {
            DlqMessages.Clear();
            _dlqCount = 0;
        }
    }
}