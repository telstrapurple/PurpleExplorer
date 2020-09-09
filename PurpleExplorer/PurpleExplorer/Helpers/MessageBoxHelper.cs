using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace PurpleExplorer.Helpers
{
    public class MessageBoxHelper
    {
        public static async Task<ButtonResult> ShowError(ButtonEnum buttons, string title, string message)
        {
            var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = buttons,
                ContentTitle = title, 
                ContentMessage = message,
                Icon = Icon.Error,
                CanResize = false,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterScreen
            });

            var applicationLifeTime = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            return await msBoxStandardWindow.ShowDialog(applicationLifeTime.MainWindow);
           
        }

        public static async Task<ButtonResult> ShowMessage(string title, string message)
        {
            var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = title,
                ContentMessage = message,
                Icon = Icon.Info,
                CanResize = false,
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterScreen
            });

            var applicationLifeTime = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            return await msBoxStandardWindow.ShowDialog(applicationLifeTime.MainWindow);
        }
    }
}
