using Avalonia;

namespace PurpleExplorer.Helpers;

/// <summary>Interface for the very Application.
/// It seems that normally in Avalonia one uses a static property like so:
/// `Application.Current` but that makes mocking impossible and ergo
/// unit testing likewise.
/// </summary>
public interface IApplicationService
{
    /// <summary>Implement as `Application.Current.Lifetime`.
    /// </summary>
    Application Current { get; }
}