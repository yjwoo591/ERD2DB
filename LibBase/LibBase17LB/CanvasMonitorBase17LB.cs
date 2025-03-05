using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase15LB;
using FDCommon.LibBase.LibBase16LB;
using FDCommon.CsBase.CsBase3CB;
using FDCommon.LibBase.LibBase12LB;

namespace FDCommon.LibBase.LibBase17LB
{
    public class CanvasMonitorBase17LB
    {
        protected readonly Logging14LB _logger;
        protected readonly GlobalEventManager15LB _eventManager;
        protected readonly CanvasMonitor16LB _canvasMonitor;
        protected readonly CancellationTokenSource _cancellationTokenSource;
        protected Task _monitorTask;
        protected bool _isRunning;
        protected Panel[] _canvasPanels;

        public CanvasMonitorBase17LB()
        {
            _logger = Logging14LB.Instance;
            _eventManager = GlobalEventManager15LB.Instance;
            _canvasMonitor = CanvasMonitor16LB.Instance;
            _cancellationTokenSource = new CancellationTokenSource();
            _canvasPanels = new Panel[3];
        }

        public virtual void StartMonitoring()
        {
            if (_isRunning) return;

            try
            {
                _logger.LogMethodEntry(nameof(StartMonitoring), GetType().Name);

                _monitorTask = Task.Factory.StartNew(
                    MonitorCanvasState,
                    _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                _isRunning = true;
                _logger.LogInformation("Canvas monitoring started");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to start canvas monitoring: {ex.Message}", ex);
                throw;
            }
        }

        protected virtual async Task MonitorCanvasState()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(500, _cancellationTokenSource.Token);
                    await CheckCanvasStatus();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in canvas monitoring: {ex.Message}", ex);
                }
            }
        }

        protected virtual async Task CheckCanvasStatus()
        {
            // 캔버스 상태 체크 기본 구현
            for (int i = 0; i < _canvasPanels.Length; i++)
            {
                if (_canvasPanels[i] != null && !_canvasPanels[i].Visible)
                {
                    await _eventManager.RaiseEventAsync(
                        GlobalEventTypes3CB.CanvasVisibilityChanged,
                        this,
                        new GlobalEventArgs3CB.CanvasEventArgs
                        {
                            CanvasId = $"Canvas{i + 1}",
                            Timestamp = DateTime.Now
                        });
                }
            }
        }

        public virtual void SetCanvasPanel(int index, Panel panel)
        {
            if (index >= 0 && index < _canvasPanels.Length)
            {
                _canvasPanels[index] = panel;

                // 캔버스 모니터에 패널 등록
                var canvasInfo = new CanvasState12LB.CanvasInfo
                {
                    Id = $"Canvas{index + 1}",
                    Title = GetCanvasTitle(index),
                    Status = CanvasState12LB.CanvasStatus.Ready,
                    IsVisible = panel.Visible
                };

                _canvasMonitor.RegisterCanvas(canvasInfo.Id, canvasInfo);
                _logger.LogInformation($"Canvas panel {index} registered");
            }
        }

        protected string GetCanvasTitle(int index)
        {
            return index switch
            {
                0 => "ERD Editor",
                1 => "Database Editor",
                2 => "ERD Diagram",
                _ => $"Canvas {index + 1}"
            };
        }

        public virtual void Shutdown()
        {
            try
            {
                _cancellationTokenSource.Cancel();
                _monitorTask?.Wait();
                _cancellationTokenSource.Dispose();
                _isRunning = false;
                _logger.LogInformation("Canvas monitor shutdown completed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during shutdown: {ex.Message}", ex);
            }
        }
    }
}