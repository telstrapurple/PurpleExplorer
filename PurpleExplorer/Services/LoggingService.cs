using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace PurpleExplorer.Services
{
    public class LoggingService : ILoggingService
    {
        private StringBuilder _log;

        public LoggingService()
        {
            _log = new StringBuilder();
        }

        public string Logs => _log.ToString();

        public void Log(string message)
        {
            _log.Append(message + "\n");
        }
    }
}
