using System.Collections.ObjectModel;
using PurpleExplorer.Models;
using ReactiveUI;

namespace PurpleExplorer.ViewModels
{
    public class AddMessageWindowViewModal : DialogViewModelBase
    {
        private string _message;
        private string _title;

        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }        
        
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public ObservableCollection<SavedMessage> SavedMessages { get; set; }
    }
}