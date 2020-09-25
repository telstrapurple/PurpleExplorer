using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace PurpleExplorer.Models
{
    [DataContract]
    public class AppState: IAppState
    {
        [DataMember] public ObservableCollection<string> SavedConnectionStrings { get; set; }
        [DataMember] public ObservableCollection<SavedMessage> SavedMessages { get; set; }

        public AppState()
        {
            SavedConnectionStrings = new ObservableCollection<string>();
            SavedMessages = new ObservableCollection<SavedMessage>();
        }
    }
}