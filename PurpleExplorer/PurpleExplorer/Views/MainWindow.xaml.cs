using Avalonia.Controls;
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
            var treeView = this.FindControl<TreeView>("tvServiceBus");
            treeView.SelectionChanged += TreeView_SelectionChanged;
        }

        private async void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mainWindowViewModel = this.DataContext as MainWindowViewModel;
            var treeView = sender as TreeView;
            if (treeView.SelectedItem is ServiceBusSubscription)
            {
                var selectedItem = treeView.SelectedItem as ServiceBusSubscription;
                await mainWindowViewModel.SetSubscripitonMessages(selectedItem);
            }

        }
    }
}