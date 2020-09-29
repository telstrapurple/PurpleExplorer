using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PurpleExplorer.ViewModels;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PurpleExplorer.Helpers
{
    public class ModalWindowHelper
    {
        public static async Task<U> ShowModalWindow<T, U>(ViewModelBase viewModel) where T : Window, new() where U : ViewModelBase
        {
            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                .Windows[0];

            T window = new T();
            window.DataContext = viewModel;
            
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            string assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            var rawUri = "/Assets/avalonia-logo.ico";
            var bitmap = new Bitmap(assets.Open(new Uri($"avares://{assemblyName}{rawUri}")));
            window.Icon = new WindowIcon(bitmap);

            await window.ShowDialog(mainWindow);
            return window.DataContext as U;
        }
    }
}
