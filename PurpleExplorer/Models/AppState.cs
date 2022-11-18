using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using PurpleExplorer.ViewModels;

namespace PurpleExplorer.Models
{
    [DataContract]
    public class AppState: ViewModelBase, IAppState
    {
        [DataMember] 
        public ObservableCollection<ServiceBusConnectionString> SavedConnectionStrings { get; set; }
        
        [DataMember] 
        public ObservableCollection<SavedMessage> SavedMessages { get; set; }

        [DataMember]
        public AppSettings AppSettings { get; set; }
        
        public AppState()
        {
            SavedConnectionStrings = new ObservableCollection<ServiceBusConnectionString>();
            SavedMessages = new ObservableCollection<SavedMessage>();
            AppSettings = new AppSettings();
        }
    }
}