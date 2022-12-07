using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PurpleExplorer.ViewModels;
using System.Threading.Tasks;

namespace PurpleExplorer.Helpers;

public static class ModalWindowHelper
{
    public static async Task ShowModalWindow<T>(ViewModelBase viewModel) where T : Window, new()
    {
        var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            .Windows[0];

        var window = new T
        {
            DataContext = viewModel
        };

        await window.ShowDialog(mainWindow);
    }
        
    public static async Task<U> ShowModalWindow<T, U>(ViewModelBase viewModel) where T : Window, new() where U : ViewModelBase
    {
        var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            .Windows[0];

        var window = new T
        {
            DataContext = viewModel
        };

        await window.ShowDialog(mainWindow);
        return window.DataContext as U;
    }
}