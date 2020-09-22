using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PurpleExplorer.Helpers;
using PurpleExplorer.Services;
using PurpleExplorer.ViewModels;
using PurpleExplorer.Views;
using ReactiveUI;
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
            Locator.CurrentMutable.RegisterLazySingleton(() => new LoggingService(), typeof(ILoggingService));

            var suspension = new AutoSuspendHelper(ApplicationLifetime);
            RxApp.SuspensionHost.CreateNewAppState = () => new MainWindowViewModel();
            RxApp.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver("appstate.json"));
            suspension.OnFrameworkInitializationCompleted();
            var state = RxApp.SuspensionHost.GetAppState<MainWindowViewModel>();

            new MainWindow { DataContext = state }.Show();

            //if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            //{
            //    desktop.MainWindow = new MainWindow
            //    {
            //        DataContext = new MainWindowViewModel()
            //    };
            //}

            base.OnFrameworkInitializationCompleted();
        }
    }
}