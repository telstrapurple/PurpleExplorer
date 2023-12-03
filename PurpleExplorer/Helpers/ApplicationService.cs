using System;
using Avalonia;

namespace PurpleExplorer.Helpers;

internal class ApplicationService : IApplicationService
{
    public Application Current { get; } = Application.Current
        ?? throw new Exception("There must be an application running.");
}