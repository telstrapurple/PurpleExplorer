using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.ViewModels;

namespace PurpleExplorer.Views;

public class MainWindow : Window
{
    private readonly IApplicationService _applicationService;
    private readonly IModalWindowService _modalWindowService;

    public MainWindow(IApplicationService applicationService, IModalWindowService modalWindowService)
    {
        _applicationService = applicationService;
        _modalWindowService = modalWindowService;

        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    protected override async void OnOpened(EventArgs e)
    {
        var mainWindowViewModel = DataContext as MainWindowViewModel
            ?? throw new Exception("Expected a main window viewmodel but found none");
        mainWindowViewModel.ShowConnectionStringWindow();
        
        base.OnOpened(e);
    }

    private async void MessagesGrid_DoubleTapped(object sender, TappedEventArgs e)
    {
        var grid = sender as DataGrid;
        var mainWindowViewModel = DataContext as MainWindowViewModel;

        if (grid?.SelectedItem == null)
        {
            return;
        }

        var viewModal = new MessageDetailsWindowViewModel
        {
            Message = grid.SelectedItem as Message, 
            ConnectionString = mainWindowViewModel.ConnectionString,
            Subscription = mainWindowViewModel.CurrentSubscription,
            Queue = mainWindowViewModel.CurrentQueue
        };

        await _modalWindowService.ShowModalWindow<MessageDetailsWindow, MessageDetailsWindowViewModel>(viewModal);
    }

    private void MessagesGrid_Tapped(object sender, TappedEventArgs e)
    {
        var grid = sender as DataGrid;
        var mainWindowViewModel = DataContext as MainWindowViewModel;

        if (grid.SelectedItem is Message message)
        {
            mainWindowViewModel.SetSelectedMessage(message);
        }
    }

    private async void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var mainWindowViewModel = DataContext as MainWindowViewModel;
        var treeView = sender as TreeView;

        ClearOtherSelections(treeView);
        mainWindowViewModel.ClearAllSelections();
            
        var selectedItem = treeView.SelectedItems.Count > 0 ? treeView.SelectedItems[0] : null;
        if (selectedItem is ServiceBusSubscription selectedSubscription)
        {
            mainWindowViewModel.SetSelectedSubscription(selectedSubscription);
            await mainWindowViewModel.FetchMessages();
            mainWindowViewModel.RefreshTabHeaders();
        }

        if (selectedItem is ServiceBusTopic selectedTopic)
        {
            mainWindowViewModel.SetSelectedTopic(selectedTopic);
        }
            
        if (selectedItem is ServiceBusQueue selectedQueue)
        {
            mainWindowViewModel.SetSelectedQueue(selectedQueue);
            await mainWindowViewModel.FetchMessages();
            mainWindowViewModel.RefreshTabHeaders();
        }
    }

    private void ClearOtherSelections(TreeView currentTreeView)
    {
        var tvQueues = this.FindControl<TreeView>("tvQueues");
        var tvTopics = this.FindControl<TreeView>("tvTopics");
        if (currentTreeView == tvQueues)
        {
            tvTopics.UnselectAll();
        }

        if (currentTreeView == tvTopics)
        {
            tvQueues.UnselectAll();
        }
    }
}