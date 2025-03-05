using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using FDCommon.CsBase.CsBase3CB;
using FDCommon.LibBase.LibBase13LB;
using FDCommon.LibBase.LibBase15LB;
using FastColoredTextBoxNS;
using FDCommon.LibBase.LibBase14LB;

namespace ERD2DB.LibBase.LibBase14LB
{
    public class ErdDesigner14LB
    {
        private static ErdDesigner14LB _instance;
        private readonly Logging14LB _logger;
        private readonly MermaidJs13LB _mermaidJs;
        private readonly GlobalEventManager15LB _eventManager;
        private readonly CodeEditor14LB _codeEditor;
        private FastColoredTextBox _erdEditor;
        private Control _currentEditorPanel;
        private string _currentFilePath;
        private bool _isInitialized;
        private const float MIN_FONT_SIZE = 8.0f;
        private const float MAX_FONT_SIZE = 72.0f;
        private const float FONT_SIZE_STEP = 1.1f;

        public event EventHandler<string> FileError;
        public event EventHandler<string> ContentChanged;
        public event EventHandler<float> FontSizeChanged;
        public event EventHandler<string> StatusChanged;

        public static ErdDesigner14LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ErdDesigner14LB();
                }
                return _instance;
            }
        }

        private ErdDesigner14LB()
        {
            _logger = Logging14LB.Instance;
            _mermaidJs = MermaidJs13LB.Instance;
            _eventManager = GlobalEventManager15LB.Instance;
            _codeEditor = new CodeEditor14LB(_logger);
            RegisterEventHandlers();
        }

        public Control CreateEditor(Panel parentPanel)
        {
            try
            {
                _logger.LogMethodEntry(nameof(CreateEditor), nameof(ErdDesigner14LB));

                if (_isInitialized)
                {
                    _logger.LogWarning("Editor already initialized");
                    return _erdEditor;
                }

                _currentEditorPanel = parentPanel;
                _erdEditor = (FastColoredTextBox)_codeEditor.EditorControl;
                _erdEditor.Dock = DockStyle.Fill;
                _erdEditor.Language = Language.Custom;
                _erdEditor.Font = new Font("Consolas", 10);
                _erdEditor.ShowLineNumbers = true;
                _erdEditor.BackColor = Color.White;

                InitializeEditorEvents();
                _isInitialized = true;

                _logger.LogInformation("Editor created successfully");
                return _erdEditor;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create editor: {ex.Message}", ex);
                throw;
            }
        }

        private void InitializeEditorEvents()
        {
            _erdEditor.TextChanged += (s, e) =>
            {
                ContentChanged?.Invoke(this, _erdEditor.Text);
                _eventManager.RaiseEventAsync(
                    GlobalEventTypes3CB.ContentChanged,
                    this,
                    new GlobalEventArgs3CB.FileEventArgs
                    {
                        FilePath = _currentFilePath,
                        Content = _erdEditor.Text,
                        Timestamp = DateTime.Now
                    });
            };

            _erdEditor.MouseWheel += (s, e) =>
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    float currentSize = _erdEditor.Font.Size;
                    float newSize = currentSize;

                    if (e.Delta > 0)
                        newSize = Math.Min(currentSize * FONT_SIZE_STEP, MAX_FONT_SIZE);
                    else
                        newSize = Math.Max(currentSize / FONT_SIZE_STEP, MIN_FONT_SIZE);

                    if (newSize != currentSize)
                    {
                        _erdEditor.Font = new Font(_erdEditor.Font.FontFamily, newSize);
                        FontSizeChanged?.Invoke(this, newSize);
                        _eventManager.RaiseEventAsync(
                            GlobalEventTypes3CB.FontChanged,
                            this,
                            newSize);
                    }
                }
            };

            _erdEditor.Resize += (s, e) =>
            {
                _eventManager.RaiseEventAsync(
                    GlobalEventTypes3CB.LayoutChanged,
                    this,
                    new GlobalEventArgs3CB.LayoutEventArgs
                    {
                        LayoutType = "EditorResize",
                        LayoutData = new
                        {
                            Width = _erdEditor.Width,
                            Height = _erdEditor.Height
                        },
                        Timestamp = DateTime.Now
                    });
            };
        }

        private void RegisterEventHandlers()
        {
            _eventManager.RegisterEvent<GlobalEventArgs3CB.FileEventArgs>(
                GlobalEventTypes3CB.FileOpened,
                HandleFileOpened);

            _eventManager.RegisterEvent<GlobalEventArgs3CB.LayoutEventArgs>(
                GlobalEventTypes3CB.LayoutChanged,
                HandleLayoutChanged);

            _eventManager.RegisterEvent<GlobalEventArgs3CB.FileEventArgs>(
                GlobalEventTypes3CB.ContentChanged,
                HandleContentChanged);
        }

        private async void HandleFileOpened(object sender, GlobalEventArgs3CB.FileEventArgs args)
        {
            try
            {
                _logger.LogMethodEntry(nameof(HandleFileOpened), nameof(ErdDesigner14LB));

                _currentFilePath = args.FilePath;
                string content = await File.ReadAllTextAsync(args.FilePath);

                var validationResult = await _mermaidJs.ValidateMermaidAsync(content);
                if (!validationResult.Success)
                {
                    await _eventManager.RaiseEventAsync(
                        GlobalEventTypes3CB.FileError,
                        this,
                        $"ERD 파일 형식이 올바르지 않습니다: {validationResult.ErrorMessage}");
                    return;
                }

                if (_erdEditor == null)
                {
                    var panel = FindEditorPanel();
                    if (panel != null)
                    {
                        CreateEditor(panel);
                    }
                    else
                    {
                        throw new InvalidOperationException("Editor panel not found");
                    }
                }

                _erdEditor.Text = content;
                _erdEditor.ClearUndo();

                StatusChanged?.Invoke(this, $"파일 열기 완료: {Path.GetFileName(args.FilePath)}");
                _logger.LogInformation($"File loaded successfully: {args.FilePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling file: {ex.Message}", ex);
                FileError?.Invoke(this, $"파일 처리 중 오류가 발생했습니다: {ex.Message}");
                await _eventManager.RaiseEventAsync(
                    GlobalEventTypes3CB.FileError,
                    this,
                    ex.Message);
            }
        }

        private void HandleLayoutChanged(object sender, GlobalEventArgs3CB.LayoutEventArgs args)
        {
            try
            {
                if (_erdEditor != null && _erdEditor.Parent != null)
                {
                    _erdEditor.Dock = DockStyle.Fill;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling layout change: {ex.Message}", ex);
            }
        }

        private void HandleContentChanged(object sender, GlobalEventArgs3CB.FileEventArgs args)
        {
            try
            {
                if (_erdEditor != null && args.Content != _erdEditor.Text)
                {
                    _erdEditor.Text = args.Content;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling content change: {ex.Message}", ex);
            }
        }

        private Panel FindEditorPanel()
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is Form mainForm)
                {
                    foreach (Control control in mainForm.Controls)
                    {
                        if (control is SplitContainer splitContainer1)
                        {
                            if (splitContainer1.Panel1.Controls.Count > 0 &&
                                splitContainer1.Panel1.Controls[0] is Panel panel)
                            {
                                return panel;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public string GetContent()
        {
            return _erdEditor?.Text ?? string.Empty;
        }

        public void SetContent(string content)
        {
            if (_erdEditor != null)
            {
                _erdEditor.Text = content;
            }
        }

        public void Clear()
        {
            if (_erdEditor != null)
            {
                _erdEditor.Clear();
                _currentFilePath = null;
            }
        }

        public float GetFontSize()
        {
            return _erdEditor?.Font.Size ?? 10.0f;
        }

        public void SetFontSize(float size)
        {
            if (_erdEditor != null && size >= MIN_FONT_SIZE && size <= MAX_FONT_SIZE)
            {
                _erdEditor.Font = new Font(_erdEditor.Font.FontFamily, size);
                FontSizeChanged?.Invoke(this, size);
            }
        }

        public Panel GetEditorPanel()
        {
            return _currentEditorPanel as Panel;
        }

        public void Dispose()
        {
            _codeEditor?.Dispose();
            _erdEditor?.Dispose();
        }
    }
}