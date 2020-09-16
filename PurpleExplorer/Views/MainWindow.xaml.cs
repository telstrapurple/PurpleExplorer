using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Azure.ServiceBus.Core;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.ViewModels;

namespace PurpleExplorer.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void MessagesGrid_DoubleTapped(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var grid = sender as DataGrid;
            var viewModal = new MessageDetailsWindowViewModel() { Message = grid.SelectedItem as Message };
            
            await ModalWindowHelper.ShowModalWindow<MessageDetailsWindow, MessageDetailsWindowViewModel>(viewModal, 700, 500);
        }

        private void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mainWindowViewModel = DataContext as MainWindowViewModel;
            var treeView = sender as TreeView;

            mainWindowViewModel.ClearSelectedSubscription();
            if (treeView.SelectedItem is ServiceBusSubscription)
            {
                var selectedItem = treeView.SelectedItem as ServiceBusSubscription;
                mainWindowViewModel.SetSelectedSubscription(selectedItem);
            }
        }
    }
}