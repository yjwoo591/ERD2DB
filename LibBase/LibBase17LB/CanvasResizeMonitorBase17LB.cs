using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase15LB;
using FDCommon.LibBase.LibBase16LB;
using FDCommon.CsBase.CsBase3CB;
using FastColoredTextBoxNS;

namespace FDCommon.LibBase.LibBase17LB
{
    public class CanvasResizeMonitorBase17LB
    {
        protected readonly Logging14LB _logger;
        protected readonly GlobalEventManager15LB _eventManager;
        protected CanvasLayoutManager16LB _layoutManager;
        protected readonly CancellationTokenSource _cancellationTokenSource;
        protected SplitContainer _splitContainer1;
        protected SplitContainer _splitContainer2;
        protected FastColoredTextBox _erdEditor;
        protected Task _monitorTask;
        protected bool _isRunning;
        protected const int CHAR_WIDTH = 8;  // 평균 문자 폭
        protected const int PADDING = 50;    // 여백

        public CanvasResizeMonitorBase17LB()
        {
            _logger = Logging14LB.Instance;
            _eventManager = GlobalEventManager15LB.Instance;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public virtual void Initialize(SplitContainer split1, SplitContainer split2, FastColoredTextBox erdEditor)
        {
            _splitContainer1 = split1;
            _splitContainer2 = split2;
            _erdEditor = erdEditor;
            _layoutManager = new CanvasLayoutManager16LB(_splitContainer1);

            if (_erdEditor != null)
            {
                _erdEditor.TextChanged += OnErdEditorTextChanged;
                _erdEditor.Resize += OnErdEditorResize;
            }

            RegisterEventHandlers();
        }

        public virtual void StartMonitoring()
        {
            if (_isRunning) return;

            try
            {
                _logger.LogMethodEntry(nameof(StartMonitoring), GetType().Name);

                _monitorTask = Task.Factory.StartNew(
                    MonitorCanvasSize,
                    _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                _isRunning = true;
                _logger.LogInformation("Canvas resize monitoring started");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to start resize monitoring: {ex.Message}", ex);
                throw;
            }
        }

        protected virtual void RegisterEventHandlers()
        {
            _eventManager.RegisterEvent<GlobalEventArgs3CB.CanvasContentEventArgs>(
                GlobalEventTypes3CB.CanvasContentUpdated,
                (sender, args) => CheckAndAdjustCanvasWidth());

            _eventManager.RegisterEvent<GlobalEventArgs3CB.LayoutEventArgs>(
                GlobalEventTypes3CB.LayoutChanged,
                (sender, args) => CheckAndAdjustCanvasWidth());
        }

        protected virtual void OnErdEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckAndAdjustCanvasWidth();
        }

        protected virtual void OnErdEditorResize(object sender, EventArgs e)
        {
            CheckAndAdjustCanvasWidth();
        }

        protected virtual async Task MonitorCanvasSize()
        {
            var previousWidth = _splitContainer1?.Width ?? 0;
            var previousTextWidth = GetMaxLineWidth();

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(100, _cancellationTokenSource.Token);

                    var currentWidth = _splitContainer1?.Width ?? 0;
                    var currentTextWidth = GetMaxLineWidth();

                    if (currentWidth != previousWidth || currentTextWidth != previousTextWidth)
                    {
                        CheckAndAdjustCanvasWidth();
                        previousWidth = currentWidth;
                        previousTextWidth = currentTextWidth;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in canvas size monitoring: {ex.Message}", ex);
                }
            }
        }

        protected virtual int GetMaxLineWidth()
        {
            if (_erdEditor == null || string.IsNullOrEmpty(_erdEditor.Text))
                return 0;

            var lines = _erdEditor.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return lines.Length > 0 ? lines.Max(line => line.Length * CHAR_WIDTH) : 0;
        }

        protected virtual void CheckAndAdjustCanvasWidth()
        {
            try
            {
                if (_erdEditor == null || _splitContainer1 == null || _splitContainer2 == null) return;

                var maxLineWidth = GetMaxLineWidth();
                var requiredWidth = maxLineWidth + PADDING;
                var totalWidth = _splitContainer1.Width;
                var maxAllowedWidth = (int)(totalWidth * 0.7); // 최대 70%까지만 허용

                if (_splitContainer1.InvokeRequired)
                {
                    _splitContainer1.Invoke(new Action(() => AdjustSplitContainers(requiredWidth, maxAllowedWidth)));
                }
                else
                {
                    AdjustSplitContainers(requiredWidth, maxAllowedWidth);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to adjust canvas width: {ex.Message}", ex);
            }
        }

        protected virtual void AdjustSplitContainers(int requiredWidth, int maxAllowedWidth)
        {
            var newWidth = Math.Min(requiredWidth, maxAllowedWidth);

            if (newWidth > _splitContainer1.Panel1.Width ||
                newWidth < _splitContainer1.Panel1.Width && newWidth > _splitContainer1.Panel1MinSize)
            {
                _splitContainer1.SplitterDistance = newWidth;

                // Canvas2 크기 조정 (남은 공간의 절반)
                var remainingWidth = _splitContainer2.Width;
                _splitContainer2.SplitterDistance = remainingWidth / 2;

                // 레이아웃 변경 이벤트 발생
                _eventManager.RaiseEventAsync(
                    GlobalEventTypes3CB.LayoutChanged,
                    this,
                    new GlobalEventArgs3CB.LayoutEventArgs
                    {
                        LayoutType = "SplitterAdjusted",
                        LayoutData = new
                        {
                            Split1Width = _splitContainer1.SplitterDistance,
                            Split2Width = _splitContainer2.SplitterDistance
                        },
                        Timestamp = DateTime.Now
                    });

                _logger.LogInformation($"Canvas width adjusted - Canvas1: {newWidth}px");
            }
        }

        public virtual void Shutdown()
        {
            try
            {
                if (_erdEditor != null)
                {
                    _erdEditor.TextChanged -= OnErdEditorTextChanged;
                    _erdEditor.Resize -= OnErdEditorResize;
                }

                _cancellationTokenSource.Cancel();
                _monitorTask?.Wait();
                _cancellationTokenSource.Dispose();
                _isRunning = false;
                _logger.LogInformation("Canvas resize monitor shutdown completed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during shutdown: {ex.Message}", ex);
            }
        }
    }
}