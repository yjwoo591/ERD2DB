using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Text;
using FDCommon.CsBase.CsBase3CB;
using static FDCommon.CsBase.CsBase3CB.LogStructure3CB;

namespace FDCommon.LibBase.LibBase14LB
{
    public class Logging14LB
    {
        private static Logging14LB _instance;
        private ILogger _logger;
        private readonly string _logPath;
        private readonly string _logFileName;
        private readonly LogConfig _config;

        public static Logging14LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Logging14LB();
                }
                return _instance;
            }
        }

        private Logging14LB()
        {
            _config = new LogConfig();
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config.LogDirectory);
            _logFileName = _config.LogFilePrefix + ".log";
            InitializeLogger();
        }

        private void InitializeLogger()
        {
            try
            {
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }

                var logFilePath = Path.Combine(_logPath, _logFileName);

                var loggerConfiguration = new LoggerConfiguration()
                    .MinimumLevel.Debug();

                if (_config.EnableConsoleLogging)
                {
                    loggerConfiguration.WriteTo.Console(
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
                }

                loggerConfiguration.WriteTo.File(logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: _config.RetainedDays,
                    encoding: Encoding.UTF8,
                    outputTemplate: _config.OutputTemplate);

                _logger = loggerConfiguration.CreateLogger();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize logger", ex);
            }
        }

        public void LogDebug(string message)
        {
            _logger?.Debug(message);
        }

        public void LogInformation(string message)
        {
            _logger?.Information(message);
        }

        public void LogWarning(string message)
        {
            _logger?.Warning(message);
        }

        public void LogError(string message, Exception ex = null)
        {
            if (ex != null)
                _logger?.Error(ex, message);
            else
                _logger?.Error(message);
        }

        public void LogMethodEntry(string methodName, string className)
        {
            _logger?.Debug("Entering method: {Method} in class: {Class}", methodName, className);
        }

        public void LogMethodExit(string methodName, string className)
        {
            _logger?.Debug("Exiting method: {Method} in class: {Class}", methodName, className);
        }
    }
}