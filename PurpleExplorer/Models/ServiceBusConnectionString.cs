namespace PurpleExplorer.Models;

public class ServiceBusConnectionString
{
    public bool UseManagedIdentity { get; set; }
    public string ConnectionString { get; set; }
    public string Name { get; set; }
}