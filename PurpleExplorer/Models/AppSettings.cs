namespace PurpleExplorer.Models;

public class AppSettings
{
    public int QueueListFetchCount { get; set; } = 100;
    public int QueueMessageFetchCount { get; set; } = 100;

    public int TopicListFetchCount { get; set; } = 100;
    public int TopicMessageFetchCount { get; set; } = 100;
}