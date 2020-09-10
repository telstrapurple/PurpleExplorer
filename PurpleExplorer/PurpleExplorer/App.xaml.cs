using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PurpleExplorer.Helpers;
using PurpleExplorer.ViewModels;
using PurpleExplorer.Views;
using Splat;

namespace PurpleExplorer
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Locator.CurrentMutable.Register(() => new ServiceBusHelper(), typeof(IServiceBusHelper));
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}