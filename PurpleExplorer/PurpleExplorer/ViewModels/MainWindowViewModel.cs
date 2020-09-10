using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using MessageBox.Avalonia.Enums;
using PurpleExplorer.Views;

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

                    ServiceBusResource newResource = new ServiceBusResource()
                    {
                        Name = namespaceInfo.Name,
                        Topics = new ObservableCollection<ServiceBusTopic>(topics)
                    };
                    
                    ConnectedServiceBuses.Add(newResource);
                }
                catch (ArgumentException)
                {
                    await MessageBoxHelper.ShowError(ButtonEnum.Ok, "Error", "The connection string is invalid.");
                }
                catch (Exception e)
                {
                    await MessageBoxHelper.ShowError(ButtonEnum.Ok, "Error", $"An error has occurred. Please try again. {e}");
                }
            }
            else
            {
                await MessageBoxHelper.ShowError(ButtonEnum.Ok, "Error", "The connection string is missing.");
            }
        }

        public async void BtnPopupCommand()
        {
            ConnectionStringWindowViewModel viewModel = new ConnectionStringWindowViewModel();

            var returnedViewModel = await ModalWindowHelper.ShowModalWindow<ConnectionStringWindow, ConnectionStringWindowViewModel>(viewModel, 700, 100);
            ConnectionString = returnedViewModel.ConnectionString;

            if (!string.IsNullOrEmpty((ConnectionString)))
            {
                try
                {
                    ServiceBusHelper helper = new ServiceBusHelper(ConnectionString);

                    var topics = await helper.GetTopics();
                    var namespaceInfo = await helper.GetNamespaceInfo();

                    ServiceBusResource newResource = new ServiceBusResource()
                    {
                        Name = namespaceInfo.Name,
                        Topics = new ObservableCollection<ServiceBusTopic>(topics)
                    };

                    ConnectedServiceBuses.Add(newResource);
                }
                catch (Exception ex)
                {
                    await MessageBoxHelper.ShowError(ButtonEnum.Ok, "Error", "The connection string is invalid.");
                }
            }
        }
    }
}