using ReactiveUI;

namespace PurpleExplorer.ViewModels
{
    public class ConnectionStringWindowViewModel : DialogViewModelBase
    {
        private string _connectionString;

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