using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace PurpleExplorer.Helpers
{
    public class MessageBoxHelper
    {
        public static async Task<ButtonResult> ShowMessageBox(ButtonEnum buttons, string title, string message, Icon icon, bool modal)
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

            if (modal)
            {
                var applicationLifeTime = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                return await msBoxStandardWindow.ShowDialog(applicationLifeTime.MainWindow);
            }
            else
                return await msBoxStandardWindow.Show();
        }
    }
}
