using System;
using System.IO;
using System.Text.Json;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase16LB
{
    public class CanvasStateConfig16LB
    {
        private static CanvasStateConfig16LB _instance;
        private readonly string _configPath;
        private readonly string _configFileName = "canvas_config.json";
        private readonly Logging14LB _logger;
        private CanvasConfig _currentConfig;

        public static CanvasStateConfig16LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CanvasStateConfig16LB();
                }
                return _instance;
            }
        }

        public class CanvasConfig
        {
            public int AutoSaveInterval { get; set; } = 300000;
            public int MaxUndoStates { get; set; } = 50;
            public bool EnableAutoSave { get; set; } = true;
            public string DefaultSaveLocation { get; set; }
            public float DefaultSplitRatio { get; set; } = 0.33f;
            public int DefaultCanvasWidth { get; set; } = 800;
            public int DefaultCanvasHeight { get; set; } = 600;
            public int DefaultFontSize { get; set; } = 12;
        }

        private CanvasStateConfig16LB()
        {
            _logger = Logging14LB.Instance;
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            LoadConfiguration();
        }

        public CanvasConfig CurrentConfig => _currentConfig;

        private void LoadConfiguration()
        {
            try
            {
                var configFilePath = Path.Combine(_configPath, _configFileName);
                if (File.Exists(configFilePath))
                {
                    var json = File.ReadAllText(configFilePath);
                    _currentConfig = JsonSerializer.Deserialize<CanvasConfig>(json);
                    _logger.LogInformation("Canvas configuration loaded successfully");
                }
                else
                {
                    _currentConfig = CreateDefaultConfig();
                    SaveConfiguration();
                    _logger.LogInformation("Created default canvas configuration");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load canvas configuration: {ex.Message}", ex);
                _currentConfig = CreateDefaultConfig();
            }
        }

        private CanvasConfig CreateDefaultConfig()
        {
            return new CanvasConfig
            {
                DefaultSaveLocation = Path.Combine(_configPath, "CanvasStates"),
                AutoSaveInterval = 300000,
                MaxUndoStates = 50,
                EnableAutoSave = true,
                DefaultSplitRatio = 0.33f,
                DefaultCanvasWidth = 800,
                DefaultCanvasHeight = 600,
                DefaultFontSize = 12
            };
        }

        public void SaveConfiguration()
        {
            try
            {
                var configFilePath = Path.Combine(_configPath, _configFileName);
                var json = JsonSerializer.Serialize(_currentConfig, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(configFilePath, json);
                _logger.LogInformation("Canvas configuration saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save canvas configuration: {ex.Message}", ex);
                throw;
            }
        }

        public void UpdateConfiguration(Action<CanvasConfig> updateAction)
        {
            try
            {
                updateAction(_currentConfig);
                SaveConfiguration();
                _logger.LogInformation("Canvas configuration updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update canvas configuration: {ex.Message}", ex);
                throw;
            }
        }
    }
}