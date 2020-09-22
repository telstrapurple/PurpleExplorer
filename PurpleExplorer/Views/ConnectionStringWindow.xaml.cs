using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DynamicData;
using PurpleExplorer.Helpers;
using PurpleExplorer.ViewModels;
using System.Linq;

namespace PurpleExplorer.Views
{
    public class ConnectionStringWindow : Window
    {
        public ConnectionStringWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public async void btnSendClick(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as ConnectionStringWindowViewModel;
            if (string.IsNullOrEmpty(dataContext.ConnectionString))
                await MessageBoxHelper.ShowError("Please enter a service bus connection string.");
            else
            {
                dataContext.Cancel = false;
                this.Close();
            }
        }

        public async void btnSaveConnectionString(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as ConnectionStringWindowViewModel;
            
            if (dataContext.SavedConnectionStrings.FirstOrDefault(x => x == dataContext.ConnectionString) != null)
                await MessageBoxHelper.ShowMessage("Duplicate connection string", "This connection string is already saved.");
            else
                dataContext.SavedConnectionStrings.Add(dataContext.ConnectionString);
        }

        private void lsbConnectionStringSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox box = sender as ListBox;
            var dataContext = this.DataContext as ConnectionStringWindowViewModel;
            dataContext.ConnectionString = (string)box.SelectedItem;
        }

        public void btnDeleteConnectionString(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as ConnectionStringWindowViewModel;
            var listBox = this.FindControl<ListBox>("lsbSavedConnectionString");
            dataContext.SavedConnectionStrings.Remove(listBox.SelectedItem as string);
        }
    }
}
