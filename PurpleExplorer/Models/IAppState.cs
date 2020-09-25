using System.Collections.ObjectModel;

namespace PurpleExplorer.Models
{
    public interface IAppState
    {
        public ObservableCollection<string> SavedConnectionStrings { get; set; }
        public ObservableCollection<SavedMessage> SavedMessages { get; set; }
    }
}