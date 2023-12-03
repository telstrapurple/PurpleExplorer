using ReactiveUI;
using System.Collections.ObjectModel;
using PurpleExplorer.Models;

namespace PurpleExplorer.ViewModels;

public class ConnectionStringWindowViewModel : DialogViewModelBase
{
    private string _name;
    private string _connectionString;
    private bool _useManagedIdentity;

    public ObservableCollection<ServiceBusConnectionString> SavedConnectionStrings { get; set; }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public string ConnectionString
    {
        get => _connectionString;
        set => this.RaiseAndSetIfChanged(ref _connectionString, value);
    }

    public bool UseManagedIdentity
    {
        get => _useManagedIdentity;
        set => this.RaiseAndSetIfChanged(ref _useManagedIdentity, value);
    }

    public ConnectionStringWindowViewModel(IAppState appState)
    {
        SavedConnectionStrings = appState.SavedConnectionStrings;
    }
}