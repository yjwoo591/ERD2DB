using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase16LB;

namespace FDCommon.LibBase.LibBase17LB
{
    public class BaseWindowManager17LB : WindowBase16LB
    {
        private SplitContainer _splitContainer1;
        private SplitContainer _splitContainer2;
        private Panel _leftPanel;
        private Panel _centerPanel;
        private Panel _rightPanel;
        private bool _needResize = true;
        protected bool _isInitializing = true;

        public Panel LeftPanel => _leftPanel;
        public Panel CenterPanel => _centerPanel;
        public Panel RightPanel => _rightPanel;

        public enum ResizeReason
        {
            FileOpen,
            WindowResize,
            FontSizeChange
        }

        protected const int DEFAULT_CHAR_WIDTH = 10;
        protected const int EXTRA_PADDING = 60;
        protected const double MIN_WIDTH_RATIO = 0.25;
        protected const double MAX_WIDTH_RATIO = 0.75;

        public bool NeedResize
        {
            get { return _needResize; }
            protected set { _needResize = value; }
        }

        public void SetResizeRequired()
        {
            _needResize = true;
            Logger.LogInformation("Resize flag set to true");
        }

        public virtual void AdjustPanelWidth(string content, ResizeReason reason)
        {
            try
            {
                if (_isInitializing || !_needResize || string.IsNullOrEmpty(content) || _splitContainer1 == null)
                    return;

                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0) return;

                int maxLength = lines.Max(line => line.Length);
                int charWidth = GetCharacterWidth(reason);

                int requiredWidth = (maxLength * charWidth) + EXTRA_PADDING;

                int availableWidth = _splitContainer1.Width - _splitContainer1.SplitterWidth;
                int minimumWidth = (int)(availableWidth * MIN_WIDTH_RATIO);
                int maximumWidth = (int)(availableWidth * MAX_WIDTH_RATIO);

                int newWidth = Math.Max(minimumWidth, Math.Min(requiredWidth, maximumWidth));

                _splitContainer1.SplitterDistance = newWidth;

                if (_splitContainer2 != null)
                {
                    _splitContainer2.SplitterDistance = _splitContainer2.Width / 2;
                }

                _needResize = false;
                Logger.LogInformation($"Adjusted panel width to {newWidth}px for max line length: {maxLength}, Reason: {reason}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in AdjustPanelWidth: {ex.Message}", ex);
            }
        }

        protected virtual int GetCharacterWidth(ResizeReason reason)
        {
            switch (reason)
            {
                case ResizeReason.FontSizeChange:
                    return (int)(DEFAULT_CHAR_WIDTH * 1.2f);
                case ResizeReason.WindowResize:
                    return DEFAULT_CHAR_WIDTH;
                case ResizeReason.FileOpen:
                    return DEFAULT_CHAR_WIDTH;
                default:
                    return DEFAULT_CHAR_WIDTH;
            }
        }

        public virtual void InitializeSplitContainers(Control parentControl)
        {
            try
            {
                _splitContainer1 = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    Orientation = Orientation.Vertical,
                    SplitterWidth = 5
                };

                _splitContainer2 = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    Orientation = Orientation.Vertical,
                    SplitterWidth = 5
                };

                _leftPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(10)
                };

                _centerPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(10)
                };

                _rightPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(10)
                };

                _splitContainer1.Panel1.Controls.Add(_leftPanel);
                _splitContainer2.Panel1.Controls.Add(_centerPanel);
                _splitContainer2.Panel2.Controls.Add(_rightPanel);

                _splitContainer1.Panel2.Controls.Add(_splitContainer2);

                SetEqualWidths();

                parentControl.Controls.Add(_splitContainer1);

                _isInitializing = false;
                Logger.LogInformation("Split containers initialized successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to initialize split containers: {ex.Message}", ex);
                throw;
            }
        }

        public virtual void SetEqualWidths()
        {
            try
            {
                if (_splitContainer1 == null || _splitContainer2 == null)
                    return;

                int totalWidth = _splitContainer1.Width;
                int availableWidth = totalWidth - (_splitContainer1.SplitterWidth * 2);
                int oneThirdWidth = availableWidth / 3;

                _splitContainer1.SplitterDistance = oneThirdWidth;
                _splitContainer2.SplitterDistance = oneThirdWidth;

                Logger.LogInformation($"Set equal widths: {oneThirdWidth}px each");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in SetEqualWidths: {ex.Message}", ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _splitContainer1?.Dispose();
                _splitContainer2?.Dispose();
                _leftPanel?.Dispose();
                _centerPanel?.Dispose();
                _rightPanel?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}