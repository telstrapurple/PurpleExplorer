using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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

        private async void MessagesGrid_DoubleTapped(object sender, RoutedEventArgs e)
        {
            var grid = sender as DataGrid;
            var mainWindowViewModel = DataContext as MainWindowViewModel;
            var connectionString = mainWindowViewModel.ConnectionString;
            var subscription = mainWindowViewModel.CurrentSubscription;
            var viewModal = new MessageDetailsWindowViewModel
            {
                Message = grid.SelectedItem as Message, 
                ConnectionString = connectionString, 
                Subscription = subscription
            };

            await ModalWindowHelper.ShowModalWindow<MessageDetailsWindow, MessageDetailsWindowViewModel>(viewModal);
        }

        private void MessagesGrid_Tapped(object sender, RoutedEventArgs e)
        {
            var grid = sender as DataGrid;
            var mainWindowViewModel = DataContext as MainWindowViewModel;

            if (grid.SelectedItem is Message message)
            {
                mainWindowViewModel.SetSelectedMessage(message);
            }
        }

        private void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mainWindowViewModel = DataContext as MainWindowViewModel;
            var treeView = sender as TreeView;
            
            mainWindowViewModel.ClearSelection();
            
            var selectedItem = treeView.SelectedItems.Count > 0 ? treeView.SelectedItems[0] : null;
            if (selectedItem is ServiceBusSubscription selectedSubscription)
            {
                mainWindowViewModel.SetSelectedSubscription(selectedSubscription);
            }

            if (selectedItem is ServiceBusTopic selectedTopic)
            {
                mainWindowViewModel.SetSelectedTopic(selectedTopic);
            }
        }
    }
}