using System.Collections.ObjectModel;
using PurpleExplorer.ViewModels;

namespace PurpleExplorer.Models
{
    public interface IAppState
    {
        public ObservableCollection<string> SavedConnectionStrings { get; set; }
        public ObservableCollection<SavedMessage> SavedMessages { get; set; }
        public AppSettings AppSettings { get; set; }
    }
}