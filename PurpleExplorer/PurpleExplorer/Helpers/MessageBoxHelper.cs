using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PurpleExplorer.Helpers
{
    public class MessageBoxHelper
    {
        public static async Task<ButtonResult> ShowMessageBox(ButtonEnum buttons, string title, string message, Icon icon)
        {
            var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = buttons,
                ContentTitle = title, 
                ContentMessage = message,
                Icon = icon,
                CanResize = false,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterScreen
            });

            return await msBoxStandardWindow.Show();
        }
    }
}
