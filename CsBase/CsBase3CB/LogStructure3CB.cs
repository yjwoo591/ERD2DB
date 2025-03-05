using System;

namespace FDCommon.CsBase.CsBase3CB
{
    public class LogStructure3CB
    {
        public enum LogLevel
        {
            Debug = 0,
            Information = 1,
            Warning = 2,
            Error = 3,
            Fatal = 4
        }

        public class LogConfig
        {
            public string LogDirectory { get; set; }
            public string LogFilePrefix { get; set; }
            public LogLevel MinimumLevel { get; set; }
            public int RetainedDays { get; set; }
            public bool EnableConsoleLogging { get; set; }
            public string OutputTemplate { get; set; }

            public LogConfig()
            {
                LogDirectory = "Logs";
                LogFilePrefix = "ERD2DB_Log_";
                MinimumLevel = LogLevel.Debug;
                RetainedDays = 31;
                EnableConsoleLogging = true;
                OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
            }
        }

        public class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public LogLevel Level { get; set; }
            public string Message { get; set; }
            public string Source { get; set; }
            public Exception Exception { get; set; }
            public string StackTrace { get; set; }

            public LogEntry()
            {
                Timestamp = DateTime.Now;
            }
        }
    }
}