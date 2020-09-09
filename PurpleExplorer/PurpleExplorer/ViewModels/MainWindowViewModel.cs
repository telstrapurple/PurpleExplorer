using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using MessageBox.Avalonia.Enums;

namespace PurpleExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<ServiceBusResource> ConnectedServiceBuses { get; }
        public ObservableCollection<Message> Messages { get; }
        public string ConnectionString { get; set; }
        public MainWindowViewModel()
        {
            ConnectedServiceBuses = new ObservableCollection<ServiceBusResource>();
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
                },
                new Message()
                {
                    Content = "Test Message 3",
                    Size = 3
                }
            };
            return mockMessages;
        }

        public async void BtnConnectCommand()
        {
            if (!string.IsNullOrEmpty(ConnectionString))
            {
                try
                {
                    ServiceBusHelper helper = new ServiceBusHelper(ConnectionString);
                    
                    var namespaceInfo = await helper.GetNamespaceInfo();
                    var topics = await helper.GetTopics();

                    ServiceBusResource newResource = new ServiceBusResource(topics)
                    {
                        Name = namespaceInfo.Name,
                    };
                    
                   ConnectedServiceBuses.Add(newResource);
                }
                catch (Exception ex)
                {
                    await MessageBoxHelper.ShowMessageBox(ButtonEnum.Ok, "Error", "The connection string is invalid.", Icon.Error);
                }
            }
            else
            {
                await MessageBoxHelper.ShowMessageBox(ButtonEnum.Ok, "Error", "The connection string is missing.", Icon.Error);
            }
        }
    }
}