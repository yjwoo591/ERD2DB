using System;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase15LB
{
    public class SplitContainerEvent15LB : SplitContainerBase15LB
    {
        public event EventHandler<SplitterEventArgs> SplitterMoved;
        private bool _isProcessingSplitterMove;

        public override void Initialize(DockStyle dockStyle, Orientation orientation, int splitterWidth)
        {
            base.Initialize(dockStyle, orientation, splitterWidth);
            AttachEventHandlers();
        }

        private void AttachEventHandlers()
        {
            if (_splitContainer != null)
            {
                _splitContainer.SplitterMoving += OnSplitterMoving;
                _splitContainer.SplitterMoved += OnSplitterMoved;
            }
        }

        private void OnSplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            if (_isProcessingSplitterMove) return;

            try
            {
                _isProcessingSplitterMove = true;
                ValidateSplitterPosition(e);
            }
            finally
            {
                _isProcessingSplitterMove = false;
            }
        }

        private void ValidateSplitterPosition(SplitterCancelEventArgs e)
        {
            if (_splitContainer == null) return;

            int minDistance = _splitContainer.Panel1MinSize;
            int maxDistance = _splitContainer.Width - _splitContainer.Panel2MinSize;

            if (e.SplitX < minDistance || e.SplitX > maxDistance)
            {
                e.Cancel = true;
            }
        }

        private void OnSplitterMoved(object sender, SplitterEventArgs e)
        {
            if (_isProcessingSplitterMove) return;

            try
            {
                _isProcessingSplitterMove = true;
                SplitterMoved?.Invoke(this, e);
                _logger.LogInformation($"Splitter moved to position: {_splitContainer.SplitterDistance}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling splitter moved event: {ex.Message}", ex);
            }
            finally
            {
                _isProcessingSplitterMove = false;
            }
        }

        public void EnableSplitter(bool enable)
        {
            if (_splitContainer != null)
            {
                _splitContainer.IsSplitterFixed = !enable;
                _logger.LogInformation($"Splitter {(enable ? "enabled" : "disabled")}");
            }
        }

        public override void Dispose()
        {
            if (_splitContainer != null)
            {
                _splitContainer.SplitterMoving -= OnSplitterMoving;
                _splitContainer.SplitterMoved -= OnSplitterMoved;
            }
            base.Dispose();
        }
    }
}