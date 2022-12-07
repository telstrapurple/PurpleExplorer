using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.ViewModels;

namespace PurpleExplorer.Views;

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

    public void btnDeleteMessage(object sender, RoutedEventArgs e)
    {
        var dataContext = DataContext as AddMessageWindowViewModal;
        var dataGrid = this.FindControl<DataGrid>("dgSavedMessages");
        dataContext.SavedMessages.Remove(dataGrid.SelectedItem as SavedMessage);
    }

    public void messageSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var dataContext = DataContext as AddMessageWindowViewModal;
        var dataGrid = sender as DataGrid;
        var selectedMessage = dataGrid.SelectedItem as SavedMessage;
        dataContext.Message = selectedMessage?.Message;
        dataContext.Title = selectedMessage?.Title;
    }
}