using PurpleExplorer.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PurpleExplorer.ViewModels
{
    public class ConnectionStringWindowViewModel : ViewModelBase
    {
        public string ConnectionString { get; set; }

        public async void BtnConnectCommand()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                await MessageBoxHelper.ShowError(MessageBox.Avalonia.Enums.ButtonEnum.OkCancel, "Error", "Please enter a service bus connection string.");
        }
    }
}
