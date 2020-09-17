using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Azure.Amqp.Framing;
using PurpleExplorer.Helpers;
using PurpleExplorer.ViewModels;

namespace PurpleExplorer.Views
{
    public class AddMessageWindow : Window
    {
        public AddMessageWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async void btnAddClick(object sender, RoutedEventArgs e)
        {
            var dataContext = DataContext as AddMessageWindowViewModal;
            if (string.IsNullOrEmpty(dataContext.Message))
                await MessageBoxHelper.ShowError("Please enter a message to be sent");
            else
            {
                dataContext.Cancel = false;
                Close();
            }
        }
    }
}