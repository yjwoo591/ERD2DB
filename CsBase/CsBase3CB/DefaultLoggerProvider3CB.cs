using System;

namespace FDCommon.CsBase.CsBase3CB
{
    public class DefaultLoggerProvider3CB : ILoggerProvider3CB
    {
        private static ILogger3CB _logger;

        public ILogger3CB GetLogger()
        {
            if (_logger == null)
            {
                _logger = new DefaultLogger3CB();
            }
            return _logger;
        }

        public void SetLogger(ILogger3CB logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }

    internal class DefaultLogger3CB : ILogger3CB
    {
        public void LogDebug(string message)
        {
            WriteLog("DEBUG", message);
        }

        public void LogInformation(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARN", message);
        }

        public void LogError(string message, Exception ex = null)
        {
            WriteLog("ERROR", message);
            if (ex != null)
            {
                WriteLog("ERROR", ex.ToString());
            }
        }

        public void LogMethodEntry(string methodName, string className)
        {
            WriteLog("DEBUG", $"Entering method: {methodName} in class: {className}");
        }

        public void LogMethodExit(string methodName, string className)
        {
            WriteLog("DEBUG", $"Exiting method: {methodName} in class: {className}");
        }

        private void WriteLog(string level, string message)
        {
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
            Console.WriteLine(logMessage);
        }
    }
}