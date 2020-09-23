using System.Threading.Tasks;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.Services;
using Splat;

namespace PurpleExplorer.ViewModels
{
    public class MessageDetailsWindowViewModel : ViewModelBase
    {
        private readonly IServiceBusHelper _serviceBusHelper;
        private readonly ILoggingService _loggingService;

        public Message Message { get; set; }
        public ServiceBusSubscription Subscription { get; set; }
        public string ConnectionString { get; set; }

        public MessageDetailsWindowViewModel(IServiceBusHelper serviceBusHelper = null,
            ILoggingService loggingService = null)
        {
            _loggingService = loggingService ?? Locator.Current.GetService<ILoggingService>();
            _serviceBusHelper = serviceBusHelper ?? Locator.Current.GetService<IServiceBusHelper>();
        }

        public async Task ResendMessage()
        {
            //TODO: log - refactor?

            await _serviceBusHelper.ResendDlqMessage(ConnectionString, Subscription.Topic.Name, Subscription.Name,
                Message);

            //TODO: log - refactor?
        }
    }
}