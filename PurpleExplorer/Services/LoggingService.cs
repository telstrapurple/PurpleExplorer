using System;
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
            _log.Insert(0, "[" + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt") + "] " + message + "\n");
        }
    }
}
