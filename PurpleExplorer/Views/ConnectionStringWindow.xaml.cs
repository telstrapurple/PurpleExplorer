using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PurpleExplorer.Helpers;
using PurpleExplorer.ViewModels;
using System.Linq;
using Avalonia;

namespace PurpleExplorer.Views;

public class ConnectionStringWindow : Window
{
    public ConnectionStringWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    public async void btnSendClick(object sender, RoutedEventArgs e)
    {
        var dataContext = DataContext as ConnectionStringWindowViewModel;
        if (string.IsNullOrEmpty(dataContext.ConnectionString))
            await MessageBoxHelper.ShowError("Please enter a service bus connection string.");
        else
        {
            dataContext.Cancel = false;
            Close();
        }
    }

    public async void btnSaveConnectionString(object sender, RoutedEventArgs e)
    {
        var dataContext = DataContext as ConnectionStringWindowViewModel;
            
        if (dataContext.SavedConnectionStrings.FirstOrDefault(x => x.ConnectionString == dataContext.ConnectionString && x.UseManagedIdentity == dataContext.UseManagedIdentity) != null)
            await MessageBoxHelper.ShowMessage("Duplicate connection string", "This connection string is already saved.");
        else
            dataContext.SavedConnectionStrings.Add(new Models.ServiceBusConnectionString
            {
                Name = dataContext.Name,
                ConnectionString = dataContext.ConnectionString,
                UseManagedIdentity = dataContext.UseManagedIdentity
            });
    }

    private void lsbConnectionStringSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var dataGrid = sender as DataGrid;
        var dataContext = DataContext as ConnectionStringWindowViewModel;
        var item = dataGrid.SelectedItem as Models.ServiceBusConnectionString;
        dataContext.Name = item.Name;
        dataContext.ConnectionString = item.ConnectionString;
        dataContext.UseManagedIdentity = item.UseManagedIdentity;
    }

    public void btnDeleteConnectionString(object sender, RoutedEventArgs e)
    {
        var dataContext = DataContext as ConnectionStringWindowViewModel;
        var dataGrid = this.FindControl<DataGrid>("lsbSavedConnectionString");
        dataContext.SavedConnectionStrings.Remove(dataGrid.SelectedItem as Models.ServiceBusConnectionString);
    }
    
    public void OnCancel(object? sender, RoutedEventArgs args)
    {
        this.Close();
    }
}