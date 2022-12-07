using ReactiveUI;
using System.Collections.ObjectModel;
using PurpleExplorer.Models;
using Splat;

namespace PurpleExplorer.ViewModels;

public class ConnectionStringWindowViewModel : DialogViewModelBase
{
    private string _connectionString;
    private bool _useManagedIdentity;
    private readonly IAppState _appState;
    public ObservableCollection<ServiceBusConnectionString> SavedConnectionStrings { get; set; }
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

    public ConnectionStringWindowViewModel(IAppState appState = null)
    {
        _appState = appState ?? Locator.Current.GetService<IAppState>();
        SavedConnectionStrings = _appState.SavedConnectionStrings;
    }

}