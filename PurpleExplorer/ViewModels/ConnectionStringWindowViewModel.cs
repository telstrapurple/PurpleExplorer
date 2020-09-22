using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PurpleExplorer.ViewModels
{
    public class ConnectionStringWindowViewModel : DialogViewModelBase
    {
        private string _connectionString;
        public ObservableCollection<string> SavedConnectionStrings { get; set; }
        public string ConnectionString
        {
            get => _connectionString;
            set => this.RaiseAndSetIfChanged(ref _connectionString, value);
        }

        public ConnectionStringWindowViewModel() : base()
        {
        }

    }
}