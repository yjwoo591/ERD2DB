using System;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase16LB
{
    public class WindowBase16LB : IDisposable
    {
        private readonly KryptonManager14LB _kryptonManager;
        private readonly Logging14LB _logger;
        private readonly CodeEditor14LB _codeEditor;
        private Panel _contentPanel;
        private MenuStrip _mainMenu;
        private StatusStrip _statusStrip;
        protected bool _isInitializing;
        private System.ComponentModel.IContainer _components;
        private bool _disposed = false;

        public KryptonManager14LB KryptonManager => _kryptonManager;
        public Logging14LB Logger => _logger;
        public CodeEditor14LB CodeEditor => _codeEditor;
        public Panel ContentPanel => _contentPanel;
        public MenuStrip MainMenu => _mainMenu;
        public StatusStrip StatusStrip => _statusStrip;

        protected bool IsInitializing
        {
            get => _isInitializing;
            set => _isInitializing = value;
        }

        protected System.ComponentModel.IContainer Components
        {
            get => _components;
            set => _components = value;
        }

        public WindowBase16LB()
        {
            _kryptonManager = KryptonManager14LB.Instance;
            _logger = Logging14LB.Instance;
            _codeEditor = new CodeEditor14LB(_logger);
            _isInitializing = true;
        }

        protected virtual void InitializeBaseComponents()
        {
            try
            {
                _components = new System.ComponentModel.Container();
                _contentPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(0, 10, 0, 0)
                };

                _mainMenu = _kryptonManager.CreateMenuStrip();
                _mainMenu.Dock = DockStyle.Top;

                _statusStrip = _kryptonManager.CreateStatusStrip();
                _statusStrip.Items.Add(new ToolStripStatusLabel("준비"));

                var fontSizeLabel = _codeEditor.GetFontSizeLabel();
                if (fontSizeLabel != null)
                {
                    _statusStrip.Items.Add(fontSizeLabel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize base components", ex);
                throw;
            }
        }

        protected virtual void SetupLayout(Form form)
        {
            try
            {
                form.SuspendLayout();

                form.Controls.Add(_contentPanel);
                form.Controls.Add(_mainMenu);
                form.Controls.Add(_statusStrip);

                _contentPanel.BringToFront();
                _statusStrip.BringToFront();
                _mainMenu.BringToFront();

                form.ResumeLayout(true);
                _contentPanel.CreateControl();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to setup layout", ex);
                throw;
            }
        }

        protected virtual void UpdateTitle(Form form)
        {
            string baseTitle = "제목 없음 - ERD2DB";
            form.Text = $"{baseTitle} [{form.Location.X},{form.Location.Y}][{form.Width}x{form.Height}]";
            _logger.LogInformation($"Updated form title: {form.Text}");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _components?.Dispose();
                    _contentPanel?.Dispose();
                    _mainMenu?.Dispose();
                    _statusStrip?.Dispose();
                    _codeEditor?.Dispose();
                }
                _disposed = true;
            }
        }

        ~WindowBase16LB()
        {
            Dispose(false);
        }
    }
}