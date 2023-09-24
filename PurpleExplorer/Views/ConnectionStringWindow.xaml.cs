using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PurpleExplorer.Helpers;
using PurpleExplorer.ViewModels;
using System.Linq;
using Avalonia;
using Avalonia.Input;

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
        SetFocus(this.FindControl<TextBox>("Name"));
    }
    
    public async void btnConnectClick(object sender, RoutedEventArgs e)
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
    
    public void OnClose(object? sender, RoutedEventArgs args)
    {
        this.Close();
    }
    
    /// <summary>This method sets the focus to the control at start time.
    /// I would prefer to do it *in xaml*,
    /// like in this issue: <see cref="https://github.com/AvaloniaUI/Avalonia/issues/4835#issuecomment-707590940"/>
    /// but I cannot get the code to work.
    /// I added
    /// "Avalonia.Xaml.Behaviors" Version="11.0.2" 
    /// "Avalonia.Xaml.Interactions" Version="11.0.2" 
    /// "Avalonia.Xaml.Interactions.Responsive" Version="11.0.2" 
    /// "Avalonia.Xaml.Interactivity" Version="11.0.2"
    /// and updated the xaml as the link says; but to no avail.
    ///
    /// I keep this information here for future me, or someone else, to stumble over and implement.
    /// </summary>
    /// <param name="ctrl"></param>
    private static void SetFocus(InputElement ctrl)
    {
        ctrl.AttachedToVisualTree += (_,_) => ctrl.Focus();
    }
}