using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FDCommon.LibBase.LibBase12LB;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase16LB
{
    public class CanvasStatePersistence16LB
    {
        private static CanvasStatePersistence16LB _instance;
        private readonly string _configPath;
        private readonly string _stateFileName = "canvas_state.json";
        private readonly string _historyFileName = "canvas_history.json";
        private readonly Logging14LB _logger;
        private readonly object _lockObject = new object();
        private Stack<CanvasStateSnapshot> _undoStack;
        private Stack<CanvasStateSnapshot> _redoStack;
        private System.Threading.Timer _autoSaveTimer;
        private System.Threading.Timer _marketDataTimer;
        private const int AUTO_SAVE_INTERVAL = 300000; // 5 minutes
        private const int MARKET_DATA_CHECK_INTERVAL = 60000; // 1 minute

        private DateTime _marketOpenTime;
        private DateTime _marketCloseTime;
        private bool _isMarketOpen;

        public static CanvasStatePersistence16LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CanvasStatePersistence16LB();
                }
                return _instance;
            }
        }

        private CanvasStatePersistence16LB()
        {
            _logger = Logging14LB.Instance;
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            _undoStack = new Stack<CanvasStateSnapshot>();
            _redoStack = new Stack<CanvasStateSnapshot>();

            // 시장 시간 초기화
            InitializeMarketTime();
            InitializeTimers();
            LoadStateHistory();
        }

        private void InitializeMarketTime()
        {
            // 기본 시장 시간 설정 (한국 시간 기준)
            _marketOpenTime = DateTime.Today.AddHours(9); // 오전 9시
            _marketCloseTime = DateTime.Today.AddHours(15).AddMinutes(30); // 오후 3시 30분
            UpdateMarketStatus();
        }

        private void InitializeTimers()
        {
            _autoSaveTimer = new System.Threading.Timer(AutoSaveCallback, null, AUTO_SAVE_INTERVAL, AUTO_SAVE_INTERVAL);
            _marketDataTimer = new System.Threading.Timer(MarketDataCallback, null, 0, MARKET_DATA_CHECK_INTERVAL);
        }

        private void MarketDataCallback(object state)
        {
            try
            {
                UpdateMarketStatus();
                if (_isMarketOpen)
                {
                    // 시장이 열려있을 때의 처리
                    SaveMarketData();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Market data processing failed: {ex.Message}", ex);
            }
        }

        private void UpdateMarketStatus()
        {
            var now = DateTime.Now;
            _isMarketOpen = now >= _marketOpenTime && now <= _marketCloseTime &&
                           now.DayOfWeek != DayOfWeek.Saturday &&
                           now.DayOfWeek != DayOfWeek.Sunday;
        }

        private void SaveMarketData()
        {
            try
            {
                var marketDataSnapshot = new MarketDataSnapshot
                {
                    Timestamp = DateTime.Now,
                    IsMarketOpen = _isMarketOpen,
                    // 추가 시장 데이터 정보
                };

                var filePath = Path.Combine(_configPath, $"market_data_{DateTime.Now:yyyyMMdd}.json");
                var json = JsonSerializer.Serialize(marketDataSnapshot, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.AppendAllText(filePath, json + Environment.NewLine);
                _logger.LogInformation($"Saved market data at {DateTime.Now}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save market data: {ex.Message}", ex);
            }
        }

        public class MarketDataSnapshot
        {
            public DateTime Timestamp { get; set; }
            public bool IsMarketOpen { get; set; }
            public Dictionary<string, object> MarketData { get; set; } = new Dictionary<string, object>();
        }

        public class CanvasStateSnapshot
        {
            public DateTime Timestamp { get; set; }
            public string Operation { get; set; }
            public Dictionary<string, CanvasState12LB.CanvasInfo> States { get; set; }
            public Dictionary<string, string> FilePaths { get; set; }
            public Dictionary<string, float> SplitRatios { get; set; }
            public MarketDataSnapshot MarketData { get; set; }
        }

        private void AutoSaveCallback(object state)
        {
            try
            {
                SaveCurrentState("AutoSave");
                _logger.LogInformation("Auto-saved canvas state");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Auto-save failed: {ex.Message}", ex);
            }
        }

        public void SaveCurrentState(string operation)
        {
            try
            {
                var snapshot = CreateStateSnapshot(operation);

                lock (_lockObject)
                {
                    _undoStack.Push(snapshot);
                    _redoStack.Clear();
                    SaveStateToFile(snapshot);
                    SaveStateHistory();
                }

                _logger.LogInformation($"Saved canvas state: {operation}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save state: {ex.Message}", ex);
                throw;
            }
        }

        private CanvasStateSnapshot CreateStateSnapshot(string operation)
        {
            return new CanvasStateSnapshot
            {
                Timestamp = DateTime.Now,
                Operation = operation,
                States = new Dictionary<string, CanvasState12LB.CanvasInfo>(),
                FilePaths = new Dictionary<string, string>(),
                SplitRatios = new Dictionary<string, float>(),
                MarketData = new MarketDataSnapshot
                {
                    Timestamp = DateTime.Now,
                    IsMarketOpen = _isMarketOpen
                }
            };
        }

        private void SaveStateToFile(CanvasStateSnapshot snapshot)
        {
            var filePath = Path.Combine(_configPath, _stateFileName);
            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(filePath, json);
        }

        private void SaveStateHistory()
        {
            var filePath = Path.Combine(_configPath, _historyFileName);
            var history = new
            {
                UndoStack = _undoStack,
                RedoStack = _redoStack
            };

            var json = JsonSerializer.Serialize(history, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(filePath, json);
        }

        private void LoadStateHistory()
        {
            try
            {
                var filePath = Path.Combine(_configPath, _historyFileName);
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var history = JsonSerializer.Deserialize<dynamic>(json);

                    _undoStack = new Stack<CanvasStateSnapshot>();
                    _redoStack = new Stack<CanvasStateSnapshot>();

                    foreach (var state in history.UndoStack.EnumerateArray())
                    {
                        _undoStack.Push(JsonSerializer.Deserialize<CanvasStateSnapshot>(state.ToString()));
                    }

                    foreach (var state in history.RedoStack.EnumerateArray())
                    {
                        _redoStack.Push(JsonSerializer.Deserialize<CanvasStateSnapshot>(state.ToString()));
                    }

                    _logger.LogInformation("Loaded state history successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load state history: {ex.Message}", ex);
                _undoStack = new Stack<CanvasStateSnapshot>();
                _redoStack = new Stack<CanvasStateSnapshot>();
            }
        }

        public bool CanUndo => _undoStack.Count > 1;
        public bool CanRedo => _redoStack.Count > 0;

        public CanvasStateSnapshot Undo()
        {
            if (!CanUndo) return null;

            lock (_lockObject)
            {
                var current = _undoStack.Pop();
                _redoStack.Push(current);
                var previous = _undoStack.Peek();
                SaveStateHistory();
                return previous;
            }
        }

        public CanvasStateSnapshot Redo()
        {
            if (!CanRedo) return null;

            lock (_lockObject)
            {
                var state = _redoStack.Pop();
                _undoStack.Push(state);
                SaveStateHistory();
                return state;
            }
        }

        public void ClearHistory()
        {
            lock (_lockObject)
            {
                var currentState = _undoStack.Count > 0 ? _undoStack.Peek() : null;
                _undoStack.Clear();
                _redoStack.Clear();
                if (currentState != null)
                {
                    _undoStack.Push(currentState);
                }
                SaveStateHistory();
            }
        }

        public void Dispose()
        {
            _autoSaveTimer?.Dispose();
            _marketDataTimer?.Dispose();
        }
    }
}