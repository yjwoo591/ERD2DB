using System;
using System.IO;
using Serilog;
using FDCommon.LibBase.LibBase13LB;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase15LB
{
    public class LogCreator15LB
    {
        private static LogCreator15LB _instance;
        private readonly string _logPath;
        private readonly string _logFilePrefix;
        private readonly ILogger13LB _logger;

        public static LogCreator15LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogCreator15LB();
                }
                return _instance;
            }
        }

        private LogCreator15LB()
        {
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _logFilePrefix = "ERD2DB_Log";
            _logger = (ILogger13LB)Logging14LB.Instance;
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

                var loggerConfiguration = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File(
                        Path.Combine(_logPath, $"{_logFilePrefix}_.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

                Log.Logger = loggerConfiguration.CreateLogger();

                _logger.LogInformation("Logger initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize logger: {ex.Message}");
                throw;
            }
        }

        public ILogger13LB GetLogger()
        {
            return _logger;
        }
    }
}