using System;
using System.Collections.Generic;
using System.Text;

namespace PurpleExplorer.Services
{
    public interface ILoggingService
    {
        void Log(string message);
        string Logs { get; }
    }
}
