using System;
using System.Configuration;
using FDCommon.LibBase.LibBase14LB;

namespace ERD2DB.Main
{
    public class Config36MA
    {
        private static Config36MA _instance;
        private readonly Logging14LB _logger;

        public static Config36MA Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Config36MA();
                }
                return _instance;
            }
        }

        private Config36MA()
        {
            _logger = Logging14LB.Instance;
        }

        public void LoadConfiguration()
        {
            _logger.LogMethodEntry(nameof(LoadConfiguration), nameof(Config36MA));
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                _logger.LogInformation("Configuration loaded successfully");
                // TODO: Add configuration loading logic
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load configuration", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(LoadConfiguration), nameof(Config36MA));
            }
        }

        public void SaveConfiguration()
        {
            _logger.LogMethodEntry(nameof(SaveConfiguration), nameof(Config36MA));
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                _logger.LogInformation("Configuration saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save configuration", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(SaveConfiguration), nameof(Config36MA));
            }
        }
    }
}