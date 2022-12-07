namespace PurpleExplorer.Services;

public interface ILoggingService
{
    void Log(string message);
    string Logs { get; }
}