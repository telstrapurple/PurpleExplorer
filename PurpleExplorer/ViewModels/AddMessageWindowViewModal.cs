using ReactiveUI;

namespace PurpleExplorer.ViewModels
{
    public class AddMessageWindowViewModal : DialogViewModelBase
    {
        private string _message;

        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public AddMessageWindowViewModal() : base()
        {
        }
    }
}