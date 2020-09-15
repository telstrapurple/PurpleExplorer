using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PurpleExplorer.Helpers;
using PurpleExplorer.ViewModels;

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
                await MessageBoxHelper.ShowError(MessageBox.Avalonia.Enums.ButtonEnum.OkCancel, "Error", "Please enter a service bus connection string.");
            else
                this.Close();
        }
    }
}
