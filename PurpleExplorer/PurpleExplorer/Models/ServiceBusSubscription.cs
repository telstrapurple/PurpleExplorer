using System.Collections.Generic;

namespace PurpleExplorer.Models
{
    public class ServiceBusSubscription
    {
        public string Name { get; set; }
        public long MessageCount { get; set; }
        public long DLQCount { get; set; }
        
        public ServiceBusTopic Topic { get; set; }

        public IList<Message> FetchActiveMessages()
        {
            return new List<Message>()
            {
                new Message()
                {
                    Content = "Test Message 1",
                    Size = 1
                },
                new Message()
                {
                    Content = "Test Message 2",
                    Size = 2
                },
                new Message()
                {
                    Content = "Test Message 3",
                    Size = 3
                }
            };
        }
        
        public IList<Message> FetchDeadLetterQueueMessages()
        {
            return new List<Message>()
            {
                new Message()
                {
                    Content = "Test Message 1",
                    Size = 1
                },
                new Message()
                {
                    Content = "Test Message 2",
                    Size = 2
                },
                new Message()
                {
                    Content = "Test Message 3",
                    Size = 3
                }
            };
        }
    }
}