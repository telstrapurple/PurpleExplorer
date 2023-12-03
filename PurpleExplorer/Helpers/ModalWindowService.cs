using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PurpleExplorer.ViewModels;
using System.Threading.Tasks;

namespace PurpleExplorer.Helpers;

public class ModalWindowService : IModalWindowService
{
    private readonly IApplicationService _applicationService;

    public ModalWindowService(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }
    
    public async Task ShowModalWindow<T>(ViewModelBase viewModel) where T : Window, new()
    {
        var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            .Windows[0];

        var window = new T
        {
            DataContext = viewModel
        };

        await window.ShowDialog(mainWindow);
    }

    public async Task<TViewModel> ShowModalWindow<TWindow,TViewModel>(ViewModelBase viewModel) 
        where TWindow : Window, new() where TViewModel : ViewModelBase
    {
        var mainWindow = (_applicationService.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                ?.Windows[0] ?? throw new Exception("Cannot find a proper window.");

        var window = new TWindow
        {
            DataContext = viewModel 
        };

        await window.ShowDialog(mainWindow);
        return (TViewModel)window.DataContext;
    }
}