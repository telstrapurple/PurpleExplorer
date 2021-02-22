using AzureMessage = Microsoft.Azure.ServiceBus.Message;

namespace PurpleExplorer.Helpers
{
    public static class Extensions
    {
        public static AzureMessage CloneMessage(this AzureMessage original)
        {
            return new AzureMessage
            {
                Body = original.Body,
                Label = original.Label,
                To = original.To,
                SessionId = original.SessionId,
                ContentType = original.ContentType
            };
        }
    }
}