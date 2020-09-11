using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Native.Interop;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using MessageBox.Avalonia.Enums;
using PurpleExplorer.Views;
using Splat;

namespace PurpleExplorer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<ServiceBusResource> ConnectedServiceBuses { get; }
        public ObservableCollection<Message> Messages { get; }
        public ObservableCollection<Message> DlqMessages { get; }
        private IServiceBusHelper ServiceBusHelper { get; }
        public MainWindowViewModel(IServiceBusHelper serviceBusHelper = null)
        {
            ServiceBusHelper = serviceBusHelper ?? Locator.Current.GetService<IServiceBusHelper>();
            ConnectedServiceBuses = new ObservableCollection<ServiceBusResource>();
            Messages = new ObservableCollection<Message>();
            DlqMessages = new ObservableCollection<Message>();
        }
        private void GenerateMockMessages(int count, int dlqCount)
        {
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                Messages.Add(new Message()
                {
                    Content = "Mocked Message " + i,
                    Size = random.Next(1, 1024)
                });
            }
            
            for (int i = 0; i < dlqCount; i++)
            {
                DlqMessages.Add(new Message()
                {
                    Content = "Mocked Message " + i,
                    Size = random.Next(1, 1024)
                });
            }
        }

        public async void BtnPopupCommand()
        {
            var viewModel = new ConnectionStringWindowViewModel();

            var returnedViewModel = await ModalWindowHelper.ShowModalWindow<ConnectionStringWindow, ConnectionStringWindowViewModel>(viewModel, 700, 100);
            var connectionString = returnedViewModel.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            try
            {
                var namespaceInfo = await ServiceBusHelper.GetNamespaceInfo(connectionString);
                var topics = await ServiceBusHelper.GetTopics(connectionString);

                var newResource = new ServiceBusResource
                {
                    Name = namespaceInfo.Name,
                    Topics = new ObservableCollection<ServiceBusTopic>(topics)
                };

                ConnectedServiceBuses.Add(newResource);
                GenerateMockMessages(8, 2);
            }
            catch (Exception ex)
            {
                await MessageBoxHelper.ShowError(ButtonEnum.Ok, "Error", "The connection string is invalid.");
            }
        }
    }
}