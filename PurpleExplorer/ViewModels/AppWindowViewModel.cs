using System.Reactive;
using PurpleExplorer.Helpers;
using ReactiveUI;

namespace PurpleExplorer.ViewModels
{
    public class AppWindowViewModel : ViewModelBase
    {
        public AppWindowViewModel()
        {
            AboutPageCommand = ReactiveCommand.Create(AboutPage);
        }

        public ReactiveCommand<Unit, Unit> AboutPageCommand { get; }

        public async void AboutPage()
        {
            await MessageBoxHelper.ShowMessage("About Purple Explorer",
                "Purple Explorer - cross-platform Azure Service Bus explorer (Windows, macOS, Linux) \n\n" +
                "Thank you for using Purple Explorer! \n " +
                "For updated information on the functionalities that Purple Explorer supports, please visit: \n " +
                "https://github.com/telstrapurple/PurpleExplorer ");
        }
    }
}