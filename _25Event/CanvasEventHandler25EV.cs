using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using FDCommon.CsBase.CsBase3CB;
using FDCommon.LibBase.LibBase13LB;
using FDCommon.LibBase.LibBase14LB;
using FastColoredTextBoxNS;

namespace ERD2DB._25Event
{
    public class CanvasEventHandler25EV
    {
        private readonly Form _mainForm;
        private readonly Logging14LB _logger;
        private readonly CanvasManager13LB _canvasManager;
        private FastColoredTextBox _erdEditor;
        private const int CHAR_WIDTH = 8;
        private const int PADDING = 50;
        private SplitContainer _splitContainer1;
        private SplitContainer _splitContainer2;

        public event EventHandler<CanvasEventArgs3CB.MainFormReadyEventArgs> MainFormReady;
        public event EventHandler<CanvasEventArgs3CB.MenuInitializedEventArgs> MenuInitialized;
        public event EventHandler<CanvasEventArgs3CB.CanvasCreatedEventArgs> CanvasCreated;
        public event EventHandler<GlobalEventArgs3CB.FileLoadedEventArgs> FileLoaded;
        public event EventHandler<GlobalEventArgs3CB.CanvasContentEventArgs> CanvasContentUpdated;

        public CanvasEventHandler25EV(Form mainForm)
        {
            _mainForm = mainForm;
            _logger = Logging14LB.Instance;
            _canvasManager = CanvasManager13LB.Instance;
            FindSplitContainers();
            InitializeEvents();
        }

        private void FindSplitContainers()
        {
            foreach (Control control in _mainForm.Controls)
            {
                if (control is Panel mainPanel)
                {
                    foreach (Control panelControl in mainPanel.Controls)
                    {
                        if (panelControl is SplitContainer splitContainer1)
                        {
                            _splitContainer1 = splitContainer1;
                            if (splitContainer1.Panel2.Controls.Count > 0 &&
                                splitContainer1.Panel2.Controls[0] is SplitContainer splitContainer2)
                            {
                                _splitContainer2 = splitContainer2;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void InitializeEvents()
        {
            try
            {
                _logger.LogMethodEntry(nameof(InitializeEvents), nameof(CanvasEventHandler25EV));

                _mainForm.Load += MainForm_Load;
                MenuInitialized += OnMenuInitialized;
                MainFormReady += OnMainFormReady;
                CanvasCreated += OnCanvasCreated;
                FileLoaded += OnFileLoaded;
                CanvasContentUpdated += OnCanvasContentUpdated;

                InitializeErdEditor();

                _logger.LogInformation("Canvas events initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize canvas events: {ex.Message}", ex);
                throw;
            }
        }

        private void InitializeErdEditor()
        {
            if (_splitContainer1?.Panel1.Controls.Count > 0)
            {
                var panel = _splitContainer1.Panel1.Controls[0];
                _erdEditor = new FastColoredTextBox
                {
                    Dock = DockStyle.Fill,
                    Language = Language.Custom,
                    Font = new Font("Consolas", 10),
                    ShowLineNumbers = true,
                    BackColor = Color.White
                };

                _erdEditor.TextChanged += ErdEditor_TextChanged;
                panel.Controls.Add(_erdEditor);
                _erdEditor.BringToFront();
            }
        }

        private void ErdEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            AdjustCanvasWidth();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                _logger.LogMethodEntry(nameof(MainForm_Load), nameof(CanvasEventHandler25EV));

                int menuHeight = 0;
                foreach (Control control in _mainForm.Controls)
                {
                    if (control is MenuStrip)
                    {
                        menuHeight = control.Height;
                        break;
                    }
                }

                MenuInitialized?.Invoke(this, new CanvasEventArgs3CB.MenuInitializedEventArgs(menuHeight, true));
                _logger.LogInformation("Main form loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in MainForm_Load: {ex.Message}", ex);
                MenuInitialized?.Invoke(this, new CanvasEventArgs3CB.MenuInitializedEventArgs(0, false));
            }
        }

        private void OnMenuInitialized(object sender, CanvasEventArgs3CB.MenuInitializedEventArgs e)
        {
            try
            {
                _logger.LogMethodEntry(nameof(OnMenuInitialized), nameof(CanvasEventHandler25EV));

                if (!e.IsSuccess)
                {
                    MessageBox.Show("메뉴 초기화에 실패했습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MainFormReady?.Invoke(this, new CanvasEventArgs3CB.MainFormReadyEventArgs(
                    _mainForm.ClientSize.Width,
                    _mainForm.ClientSize.Height,
                    e.MenuHeight));

                _logger.LogInformation("Menu initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in OnMenuInitialized: {ex.Message}", ex);
            }
        }

        private void OnMainFormReady(object sender, CanvasEventArgs3CB.MainFormReadyEventArgs e)
        {
            try
            {
                _logger.LogMethodEntry(nameof(OnMainFormReady), nameof(CanvasEventHandler25EV));

                var canvas = _canvasManager.CreateMainCanvas(_mainForm);
                if (canvas != null)
                {
                    CanvasCreated?.Invoke(this, new CanvasEventArgs3CB.CanvasCreatedEventArgs(
                        canvas.Width,
                        canvas.Height,
                        true));

                    _logger.LogInformation("Main canvas created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in OnMainFormReady: {ex.Message}", ex);
            }
        }

        private void OnCanvasCreated(object sender, CanvasEventArgs3CB.CanvasCreatedEventArgs e)
        {
            if (e.IsMainCanvas)
            {
                var erdContent = GetErdContent();
                if (!string.IsNullOrEmpty(erdContent))
                {
                    CanvasContentUpdated?.Invoke(this, new GlobalEventArgs3CB.CanvasContentEventArgs
                    {
                        CanvasId = "Canvas1",
                        Content = erdContent,
                        Timestamp = DateTime.Now
                    });
                }
            }
        }

        private void OnFileLoaded(object sender, GlobalEventArgs3CB.FileLoadedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Content))
            {
                CanvasContentUpdated?.Invoke(this, new GlobalEventArgs3CB.CanvasContentEventArgs
                {
                    CanvasId = "Canvas1",
                    Content = e.Content,
                    Timestamp = DateTime.Now
                });
            }
        }

        private void OnCanvasContentUpdated(object sender, GlobalEventArgs3CB.CanvasContentEventArgs e)
        {
            if (e.CanvasId == "Canvas1" && _erdEditor != null)
            {
                if (_erdEditor.InvokeRequired)
                {
                    _erdEditor.Invoke(new Action(() => UpdateEditorContent(e.Content)));
                }
                else
                {
                    UpdateEditorContent(e.Content);
                }
            }
        }

        private void UpdateEditorContent(string content)
        {
            try
            {
                _erdEditor.Text = content;
                _erdEditor.Refresh();
                AdjustCanvasWidth();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update editor content: {ex.Message}", ex);
            }
        }

        private string GetErdContent()
        {
            try
            {
                if (_erdEditor != null)
                {
                    return _erdEditor.Text;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get ERD content: {ex.Message}", ex);
            }
            return string.Empty;
        }

        private void AdjustCanvasWidth()
        {
            try
            {
                if (_erdEditor == null || _splitContainer1 == null) return;

                var lines = _erdEditor.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0) return;

                int maxLength = lines.Max(line => line.Length);
                int requiredWidth = (maxLength * CHAR_WIDTH) + PADDING;

                int totalWidth = _splitContainer1.Width;
                int maxAllowedWidth = (int)(totalWidth * 0.7); // 최대 70%까지만 허용
                int newWidth = Math.Min(requiredWidth, maxAllowedWidth);

                if (newWidth > _splitContainer1.Panel1.Width)
                {
                    _splitContainer1.SplitterDistance = newWidth;

                    // 나머지 공간을 2등분
                    if (_splitContainer2 != null)
                    {
                        _splitContainer2.SplitterDistance = _splitContainer2.Width / 2;
                    }

                    _logger.LogInformation($"Canvas width adjusted to {newWidth}px");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to adjust canvas width: {ex.Message}", ex);
            }
        }

        public void HandleResize()
        {
            try
            {
                AdjustCanvasWidth();
                _logger.LogInformation("Canvas layout adjusted after resize");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adjusting canvas layout: {ex.Message}", ex);
            }
        }

        public void UpdateZoom(float zoomFactor)
        {
            try
            {
                if (_erdEditor != null)
                {
                    var newSize = _erdEditor.Font.Size * zoomFactor;
                    _erdEditor.Font = new Font(_erdEditor.Font.FontFamily, newSize);
                    AdjustCanvasWidth();
                    _logger.LogInformation($"Zoom updated to {zoomFactor}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update zoom: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            try
            {
                _erdEditor?.Dispose();
                _logger.LogInformation("Canvas event handler disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error disposing canvas event handler: {ex.Message}", ex);
            }
        }
    }
}