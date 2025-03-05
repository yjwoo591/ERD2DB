using System;

namespace FDCommon.CsBase.CsBase3CB
{
    public interface ILogger3CB
    {
        void LogDebug(string message);
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
        void LogMethodEntry(string methodName, string className);
        void LogMethodExit(string methodName, string className);
    }

    public interface ILogConfiguration3CB
    {
        string LogDirectory { get; }
        string LogFilePrefix { get; }
        int RetainedDays { get; }
        bool EnableConsoleLogging { get; }
        string OutputTemplate { get; }
    }

    public interface ILoggerProvider3CB
    {
        ILogger3CB GetLogger();
        void SetLogger(ILogger3CB logger);
    }
}