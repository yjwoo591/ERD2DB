using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase12LB;
using FDCommon.LibBase.LibBase13LB;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase16LB
{
    public class CanvasMonitor16LB
    {
        private static CanvasMonitor16LB _instance;
        private readonly KryptonManager14LB _kryptonManager;
        private readonly Logging14LB _logger;
        private readonly CanvasManager13LB _canvasManager;
        private readonly ConcurrentDictionary<string, CanvasState12LB.CanvasInfo> _canvasStates;
        private readonly BlockingCollection<MonitorMessage> _messageQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task _monitorTask;
        private const int CHECK_INTERVAL_MS = 100;

        private class MonitorMessage
        {
            public string CanvasId { get; set; }
            public MonitorMessageType Type { get; set; }
            public CanvasState12LB.CanvasInfo State { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private enum MonitorMessageType
        {
            StateCheck,
            VisibilityCheck,
            PerformanceCheck,
            SizeCheck
        }

        public static CanvasMonitor16LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CanvasMonitor16LB();
                }
                return _instance;
            }
        }

        private CanvasMonitor16LB()
        {
            _kryptonManager = KryptonManager14LB.Instance;
            _logger = Logging14LB.Instance;
            _canvasManager = CanvasManager13LB.Instance;
            _canvasStates = new ConcurrentDictionary<string, CanvasState12LB.CanvasInfo>();
            _messageQueue = new BlockingCollection<MonitorMessage>();
            _cancellationTokenSource = new CancellationTokenSource();
            StartMonitoring();
        }

        private void StartMonitoring()
        {
            try
            {
                _logger.LogMethodEntry(nameof(StartMonitoring), nameof(CanvasMonitor16LB));

                _monitorTask = Task.Run(async () =>
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await MonitorCanvasStates();
                        await Task.Delay(CHECK_INTERVAL_MS, _cancellationTokenSource.Token);
                    }
                }, _cancellationTokenSource.Token);

                _logger.LogInformation("Canvas monitoring started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to start monitoring: {ex.Message}", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(StartMonitoring), nameof(CanvasMonitor16LB));
            }
        }

        private async Task MonitorCanvasStates()
        {
            try
            {
                foreach (var state in _canvasStates.Values)
                {
                    await CheckCanvasState(state);
                    LogCanvasMetrics(state);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error monitoring canvas states: {ex.Message}", ex);
            }
        }

        private async Task CheckCanvasState(CanvasState12LB.CanvasInfo state)
        {
            if (!state.IsVisible && !state.IsForceClosed)
            {
                var message = new MonitorMessage
                {
                    CanvasId = state.Id,
                    Type = MonitorMessageType.StateCheck,
                    State = state,
                    Timestamp = DateTime.Now
                };

                _messageQueue.Add(message);
                await RestoreCanvasState(state);
            }
        }

        private void LogCanvasMetrics(CanvasState12LB.CanvasInfo state)
        {
            var size = state.Position.Size.Width * state.Position.Size.Height;
            _logger.LogInformation($"Canvas {state.Id} size: {size} pixels");
        }

        private async Task RestoreCanvasState(CanvasState12LB.CanvasInfo state)
        {
            try
            {
                _logger.LogMethodEntry(nameof(RestoreCanvasState), nameof(CanvasMonitor16LB));

                state.Status = CanvasState12LB.CanvasStatus.Initializing;

                var panel = _kryptonManager.CreatePanel();
                panel.Dock = DockStyle.Fill;

                var canvas = _canvasManager.GetCanvasByName(state.Id);
                if (canvas != null)
                {
                    await Task.Run(() =>
                    {
                        if (canvas.InvokeRequired)
                        {
                            canvas.Invoke(new Action(() =>
                            {
                                canvas.Controls.Clear();
                                canvas.Controls.Add(panel);
                                canvas.Visible = true;
                            }));
                        }
                        else
                        {
                            canvas.Controls.Clear();
                            canvas.Controls.Add(panel);
                            canvas.Visible = true;
                        }
                    });
                }

                state.Status = CanvasState12LB.CanvasStatus.Ready;
                state.IsVisible = true;

                _logger.LogInformation($"Canvas {state.Id} restored successfully");
            }
            catch (Exception ex)
            {
                state.Status = CanvasState12LB.CanvasStatus.Error;
                _logger.LogError($"Failed to restore canvas state: {ex.Message}", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(RestoreCanvasState), nameof(CanvasMonitor16LB));
            }
        }

        public void RegisterCanvas(string canvasId, CanvasState12LB.CanvasInfo state)
        {
            try
            {
                _logger.LogMethodEntry(nameof(RegisterCanvas), nameof(CanvasMonitor16LB));

                if (_canvasStates.TryAdd(canvasId, state))
                {
                    _logger.LogInformation($"Canvas {canvasId} registered successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to register canvas: {ex.Message}", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(RegisterCanvas), nameof(CanvasMonitor16LB));
            }
        }

        public void Dispose()
        {
            try
            {
                _logger.LogMethodEntry(nameof(Dispose), nameof(CanvasMonitor16LB));

                _cancellationTokenSource.Cancel();
                _messageQueue.CompleteAdding();
                _monitorTask?.Wait();
                _cancellationTokenSource.Dispose();
                _messageQueue.Dispose();

                _logger.LogInformation("Canvas monitor disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error disposing canvas monitor", ex);
            }
            finally
            {
                _logger.LogMethodExit(nameof(Dispose), nameof(CanvasMonitor16LB));
            }
        }
    }
}