using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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