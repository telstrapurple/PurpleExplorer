using System.Collections.Generic;
using System.Collections.ObjectModel;
using PurpleExplorer.Models;

namespace PurpleExplorer.ViewModels
{
    public class SidePanelViewModel: ViewModelBase
    {
        public ObservableCollection<Message> Messages { get; }

        public SidePanelViewModel()
        {
            Messages = new ObservableCollection<Message>(GenerateMockMessages());
        }

        private IEnumerable<Message> GenerateMockMessages()
        {
            var mockMessages = new List<Message>()
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
                }
            };
            return mockMessages;
        }
    }
}