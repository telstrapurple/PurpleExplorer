using System.Threading.Tasks;
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

        private async void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mainWindowViewModel = DataContext as MainWindowViewModel;
            var treeView = sender as TreeView;
            mainWindowViewModel.ClearAllMessages();
            
            if (treeView.SelectedItem is ServiceBusSubscription)
            {
                var selectedItem = treeView.SelectedItem as ServiceBusSubscription;
                await Task.WhenAll(
                    mainWindowViewModel.SetSubscripitonMessages(selectedItem),
                    mainWindowViewModel.SetDlqMessages(selectedItem));
            }
        }
    }
}