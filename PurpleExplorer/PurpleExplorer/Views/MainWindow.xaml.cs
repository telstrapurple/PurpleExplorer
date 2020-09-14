using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DynamicData;
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
        
        public void tv_tapped(object sender, RoutedEventArgs e)
        {
            var mainWindowViewModel = this.DataContext as MainWindowViewModel;
            var treeView = sender as TreeView;
            var selectedItem = treeView.SelectedItem;

            //mainWindowViewModel.ClearMessages();
            if (selectedItem != null && selectedItem is ServiceBusSubscription)
            {
                var subscription = selectedItem as ServiceBusSubscription;
                mainWindowViewModel.FillMessages(subscription.Name, subscription.Topic.Name);
            }
        }
    }
}