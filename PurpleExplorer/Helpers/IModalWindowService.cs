using System.Threading.Tasks;
using Avalonia.Controls;
using PurpleExplorer.ViewModels;

namespace PurpleExplorer.Helpers;

public interface IModalWindowService
{
    Task ShowModalWindow<T>(ViewModelBase viewModel) where T : Window, new();

    Task<TViewModel> ShowModalWindow<TWindow, TViewModel>(ViewModelBase viewModel)
        where TWindow : Window, new() where TViewModel : ViewModelBase;
}