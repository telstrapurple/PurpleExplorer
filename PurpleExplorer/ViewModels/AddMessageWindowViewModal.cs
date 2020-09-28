using System.Collections.ObjectModel;
using PurpleExplorer.Models;
using ReactiveUI;
using Splat;

namespace PurpleExplorer.ViewModels
{
    public class AddMessageWindowViewModal : DialogViewModelBase
    {
        private string _message;
        private string _title;
        private readonly IAppState _appState;

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

        public AddMessageWindowViewModal(IAppState appState = null)
        {
            _appState = appState ?? Locator.Current.GetService<IAppState>();
            SavedMessages = _appState.SavedMessages;
        }

        public void SaveMessage()
        {
            var newMessage = new SavedMessage
            {
                Message = Message,
                Title = Title
            };
            SavedMessages.Add(newMessage);
        }
    }
}