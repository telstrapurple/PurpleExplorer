using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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

        public void btnSaveMessage(object sender, RoutedEventArgs e)
        {
            var dataContext = DataContext as AddMessageWindowViewModal;
            var currentMessage = dataContext.Message;
            dataContext.SavedMessages.Add(currentMessage);
        }

        public void btnDeleteMessage(object sender, RoutedEventArgs e)
        {
            var dataContext = DataContext as AddMessageWindowViewModal;
            var listbox = this.FindControl<ListBox>("lsbSavedMessages");
            dataContext.SavedMessages.Remove(listbox.SelectedItem as string);
        }

        public void messageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dataContext = DataContext as AddMessageWindowViewModal;
            var listbox = sender as ListBox;
            dataContext.Message = listbox.SelectedItem as string;
        }
    }
}