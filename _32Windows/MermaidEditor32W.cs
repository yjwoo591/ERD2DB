using System;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;

namespace ERD2DB.Windows
{
    public class MermaidEditor32W : Control
    {
        private readonly Logging14LB _logger;
        private readonly CodeEditor14LB _codeEditor;

        public string Text
        {
            get => _codeEditor.Text;
            set => _codeEditor.Text = value;
        }

        public event EventHandler TextChanged;

        public MermaidEditor32W()
        {
            _logger = Logging14LB.Instance;
            _codeEditor = new CodeEditor14LB(_logger);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            try
            {
                _codeEditor.TextChanged += (s, e) => TextChanged?.Invoke(this, EventArgs.Empty);
                _codeEditor.EditorControl.Dock = DockStyle.Fill;
                Controls.Add(_codeEditor.EditorControl);

                _logger.LogInformation("MermaidEditor components initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize MermaidEditor components", ex);
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _codeEditor?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}