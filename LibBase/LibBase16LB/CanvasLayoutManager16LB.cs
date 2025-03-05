using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using FDCommon.LibBase.LibBase14LB;
using FastColoredTextBoxNS;

namespace FDCommon.LibBase.LibBase16LB
{
    public class CanvasLayoutManager16LB
    {
        private static CanvasLayoutManager16LB _instance;
        private readonly Logging14LB _logger;
        private readonly Control _mainControl;
        private bool _isInitialized;
        private SplitContainer _splitContainer1;
        private SplitContainer _splitContainer2;
        private Panel _leftPanel;
        private Panel _centerPanel;
        private Panel _rightPanel;
        private FastColoredTextBox _erdEditor;

        public enum ResizeReason
        {
            FileOpen,
            WindowResize,
            FontChange,
            TextChange
        }

        protected const int DEFAULT_CHAR_WIDTH = 10;
        protected const int EXTRA_PADDING = 60;
        protected const double MIN_WIDTH_RATIO = 0.25;
        protected const double MAX_WIDTH_RATIO = 0.75;

        public static CanvasLayoutManager16LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CanvasLayoutManager16LB(null);
                }
                return _instance;
            }
        }

        public CanvasLayoutManager16LB(Control mainControl)
        {
            _logger = Logging14LB.Instance;
            _mainControl = mainControl;
            _isInitialized = false;
            InitializeEventHandlers();
        }

        private void InitializeEventHandlers()
        {
            // Event handlers will be initialized when needed
        }

        public void Initialize(Control parentControl)
        {
            if (_isInitialized)
                return;

            try
            {
                parentControl.SuspendLayout();

                InitializeSplitContainers(parentControl);
                InitializePanels();
                SetupLayout();

                parentControl.ResumeLayout(true);
                _isInitialized = true;
                _logger.LogInformation("Canvas layout manager initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize canvas layout manager: {ex.Message}", ex);
                throw;
            }
        }

        private void InitializeSplitContainers(Control parentControl)
        {
            _splitContainer1 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 5,
                Panel1MinSize = 100,
                Panel2MinSize = 100
            };

            _splitContainer2 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 5,
                Panel1MinSize = 100,
                Panel2MinSize = 100
            };

            _splitContainer1.Panel2.Controls.Add(_splitContainer2);
            parentControl.Controls.Add(_splitContainer1);

            _splitContainer1.SplitterMoved += SplitContainer_SplitterMoved;
            _splitContainer2.SplitterMoved += SplitContainer_SplitterMoved;
        }

        private void InitializePanels()
        {
            _leftPanel = CreatePanel("ERD Editor");
            _centerPanel = CreatePanel("Database Editor");
            _rightPanel = CreatePanel("ERD Diagram");

            _splitContainer1.Panel1.Controls.Add(_leftPanel);
            _splitContainer2.Panel1.Controls.Add(_centerPanel);
            _splitContainer2.Panel2.Controls.Add(_rightPanel);
        }

        private Panel CreatePanel(string title)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var label = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding = new Padding(5)
            };

            panel.Controls.Add(label);
            return panel;
        }

        private void SetupLayout()
        {
            SetEqualWidths();
            InitializeErdEditor();
        }

        private void InitializeErdEditor()
        {
            _erdEditor = new FastColoredTextBox
            {
                Dock = DockStyle.Fill,
                Language = Language.Custom,
                Font = new Font("Consolas", 10),
                ShowLineNumbers = true,
                BackColor = Color.White
            };

            _leftPanel.Controls.Add(_erdEditor);
        }

        public void AdjustPanelWidth(string content, ResizeReason reason)
        {
            if (!_isInitialized || string.IsNullOrEmpty(content))
                return;

            try
            {
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0) return;

                int maxLength = lines.Max(line => line.Length);
                int charWidth = GetCharacterWidth(reason);
                int requiredWidth = (maxLength * charWidth) + EXTRA_PADDING;

                int availableWidth = _mainControl?.Width ?? 800;
                int minimumWidth = (int)(availableWidth * MIN_WIDTH_RATIO);
                int maximumWidth = (int)(availableWidth * MAX_WIDTH_RATIO);

                int newWidth = Math.Max(minimumWidth, Math.Min(requiredWidth, maximumWidth));

                if (_splitContainer1.InvokeRequired)
                {
                    _splitContainer1.Invoke(new Action(() =>
                    {
                        _splitContainer1.SplitterDistance = newWidth;
                        AdjustSecondSplitContainer();
                    }));
                }
                else
                {
                    _splitContainer1.SplitterDistance = newWidth;
                    AdjustSecondSplitContainer();
                }

                _logger.LogInformation($"Panel width adjusted: {newWidth}px, Reason: {reason}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to adjust panel width: {ex.Message}", ex);
            }
        }

        private void AdjustSecondSplitContainer()
        {
            int remainingWidth = _splitContainer2.Width;
            _splitContainer2.SplitterDistance = remainingWidth / 2;
        }

        private int GetCharacterWidth(ResizeReason reason)
        {
            return reason switch
            {
                ResizeReason.FontChange => (int)(DEFAULT_CHAR_WIDTH * 1.2f),
                _ => DEFAULT_CHAR_WIDTH
            };
        }

        private void SplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (!_isInitialized) return;

            try
            {
                if (sender == _splitContainer1)
                {
                    AdjustSecondSplitContainer();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling splitter move: {ex.Message}", ex);
            }
        }

        public void SetEqualWidths()
        {
            if (!_isInitialized || _splitContainer1 == null || _splitContainer2 == null)
                return;

            try
            {
                int totalWidth = _splitContainer1.Width;
                int availableWidth = totalWidth - (_splitContainer1.SplitterWidth * 2);
                int oneThirdWidth = availableWidth / 3;

                if (_splitContainer1.InvokeRequired)
                {
                    _splitContainer1.Invoke(new Action(() =>
                    {
                        _splitContainer1.SplitterDistance = oneThirdWidth;
                        _splitContainer2.SplitterDistance = oneThirdWidth;
                    }));
                }
                else
                {
                    _splitContainer1.SplitterDistance = oneThirdWidth;
                    _splitContainer2.SplitterDistance = oneThirdWidth;
                }

                _logger.LogInformation($"Set equal widths: {oneThirdWidth}px each");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to set equal widths: {ex.Message}", ex);
            }
        }

        public void HandleResize()
        {
            if (_isInitialized)
            {
                SetEqualWidths();
            }
        }

        public void Dispose()
        {
            if (_isInitialized)
            {
                _splitContainer1?.Dispose();
                _splitContainer2?.Dispose();
                _leftPanel?.Dispose();
                _centerPanel?.Dispose();
                _rightPanel?.Dispose();
                _erdEditor?.Dispose();
                _isInitialized = false;
            }
        }
    }
}