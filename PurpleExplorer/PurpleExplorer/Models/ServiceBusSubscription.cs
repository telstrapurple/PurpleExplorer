namespace PurpleExplorer.Models
{
    public class ServiceBusSubscription
    {
        public string Name { get; set; }
        public long MessageCount { get; set; }
        public long DLQCount { get; set; }
        public string Parent { get; set; }
    }
}