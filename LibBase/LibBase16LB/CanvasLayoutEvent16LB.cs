using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase12LB;

namespace FDCommon.LibBase.LibBase16LB
{
    public class CanvasLayoutEvent16LB : CanvasLayoutUI16LB
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task _layoutMonitoringTask;
        private readonly CanvasState12LB.CanvasContainer _canvasContainer;

        public CanvasLayoutEvent16LB()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _canvasContainer = new CanvasState12LB.CanvasContainer();
        }

        public override void Initialize(Control parentControl)
        {
            base.Initialize(parentControl);
            AttachEventHandlers();
            StartLayoutMonitoring();
        }

        protected virtual void AttachEventHandlers()
        {
            _splitContainer1.SplitterMoved += SplitContainer_SplitterMoved;
            _splitContainer2.SplitterMoved += SplitContainer_SplitterMoved;
        }

        protected virtual void SplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (_isInitializing) return;

            try
            {
                if (sender == _splitContainer1.SplitContainer)
                {
                    AdjustSecondSplitContainer();
                }

                SaveLayoutState();
                _logger.LogInformation("Split container layout updated after splitter move");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling splitter move: {ex.Message}", ex);
            }
        }

        protected virtual void AdjustSecondSplitContainer()
        {
            int remainingWidth = _splitContainer1.SplitContainer.Panel2.Width;
            _splitContainer2.SetSplitterDistance(remainingWidth / 2);
        }

        protected virtual void SaveLayoutState()
        {
            var layoutState = new CanvasState12LB.CanvasInfo
            {
                Id = "LayoutState",
                Position = new ERD2DB.CsBase.CsBase1CB.GeometryBase1CB.Rectangle1CB(
                    new ERD2DB.CsBase.CsBase1CB.GeometryBase1CB.Point1CB(0, 0),
                    new ERD2DB.CsBase.CsBase1CB.GeometryBase1CB.Size1CB(
                        _splitContainer1.SplitContainer.Width,
                        _splitContainer1.SplitContainer.Height
                    )
                )
            };

            _canvasContainer.MainCanvas = layoutState;
        }

        private void StartLayoutMonitoring()
        {
            _layoutMonitoringTask = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(100, _cancellationTokenSource.Token);
                        await MonitorLayout();
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Layout monitoring error: {ex.Message}", ex);
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        protected virtual async Task MonitorLayout()
        {
            try
            {
                await UpdateLayoutAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating layout: {ex.Message}", ex);
            }
        }

        protected virtual async Task UpdateLayoutAsync()
        {
            if (_splitContainer1.SplitContainer?.InvokeRequired ?? false)
            {
                await Task.Run(() =>
                {
                    _splitContainer1.SplitContainer.Invoke(new Action(() =>
                    {
                        UpdateCanvasLayout();
                    }));
                });
            }
            else
            {
                UpdateCanvasLayout();
            }
        }

        protected virtual void UpdateCanvasLayout()
        {
            if (_splitContainer1.SplitContainer == null) return;

            var width = _splitContainer1.SplitContainer.Width;
            var height = _splitContainer1.SplitContainer.Height;
            _canvasContainer.UpdateLayout(width, height);
        }

        public override void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _layoutMonitoringTask?.Wait();
            _cancellationTokenSource.Dispose();
            base.Dispose();
        }
    }
}