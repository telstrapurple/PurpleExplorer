using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace PurpleExplorer.Helpers;

public static class MessageBoxHelper
{
    public static async Task<ButtonResult> ShowConfirmation(string title, string message)
    {
        return await ShowMessageBox(ButtonEnum.YesNo, Icon.Warning, title, message);
    }
        
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static async Task<ButtonResult> ShowError(string message)
    {
        return await ShowMessageBox(ButtonEnum.Ok, Icon.Error, "Error", message);
    }

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static async Task<ButtonResult> ShowMessage(string title, string message)
    {
        return await ShowMessageBox(ButtonEnum.Ok, Icon.Info, title, message);
    }

    private static async Task<ButtonResult> ShowMessageBox(ButtonEnum buttons, Icon icon, string title, string message)
    {
        var msBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
        {
            ButtonDefinitions = buttons,
            ContentTitle = title,
            ContentMessage = message,
            ShowInCenter = true,
            Icon = icon,
            CanResize = true,
            WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner
        });

        return await msBoxStandardWindow.ShowDialog((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            .Windows[0]);
    }
}