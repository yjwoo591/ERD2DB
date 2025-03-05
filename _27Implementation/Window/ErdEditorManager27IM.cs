using System;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase16LB;

namespace ERD2DB.Implementation.Window
{
    public class ErdEditorManager27IM
    {
        private readonly Control mainPanel;
        private Control erdEditor;
        private string currentFilePath;
        private readonly Logging14LB _logger;
        private readonly EditorManager16LB _editorManager;

        public event EventHandler<string> ErdContentChanged;

        public ErdEditorManager27IM(Control mainPanel)
        {
            this.mainPanel = mainPanel;
            _logger = Logging14LB.Instance;
            _editorManager = EditorManager16LB.Instance;
        }

        public void InitializeEditor()
        {
            _logger.LogMethodEntry(nameof(InitializeEditor), nameof(ErdEditorManager27IM));
            try
            {
                mainPanel.SuspendLayout();

                if (erdEditor != null)
                {
                    mainPanel.Controls.Remove(erdEditor);
                    erdEditor.Dispose();
                }

                erdEditor = _editorManager.CreateCodeEditor();
                erdEditor.Dock = DockStyle.Fill;
                erdEditor.Visible = true;

                _logger.LogInformation($"Editor created: Location[{erdEditor.Location.X},{erdEditor.Location.Y}] Size[{erdEditor.Width}x{erdEditor.Height}]");

                mainPanel.Controls.Add(erdEditor);
                erdEditor.BringToFront();
                erdEditor.Focus();

                mainPanel.ResumeLayout(true);
                mainPanel.Refresh();

                _logger.LogInformation("Editor initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize editor", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(InitializeEditor), nameof(ErdEditorManager27IM));
            }
        }

        public void LoadFile(string filePath)
        {
            _logger.LogMethodEntry(nameof(LoadFile), nameof(ErdEditorManager27IM));
            try
            {
                if (erdEditor == null || !mainPanel.Controls.Contains(erdEditor))
                {
                    InitializeEditor();
                }

                currentFilePath = filePath;
                string content = System.IO.File.ReadAllText(filePath);
                _editorManager.SetEditorText(erdEditor, content);

                erdEditor.Visible = true;
                erdEditor.BringToFront();
                erdEditor.Focus();

                mainPanel.Refresh();

                _logger.LogInformation($"File loaded: {filePath}");
                _logger.LogInformation($"Editor state: Location[{erdEditor.Location.X},{erdEditor.Location.Y}] Size[{erdEditor.Width}x{erdEditor.Height}] Visible[{erdEditor.Visible}]");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load file: {filePath}", ex);
                MessageBox.Show($"ERD 파일을 불러오는 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _logger.LogMethodExit(nameof(LoadFile), nameof(ErdEditorManager27IM));
            }
        }

        public Control GetEditor()
        {
            return erdEditor;
        }

        public string GetContent()
        {
            if (erdEditor == null || !erdEditor.Visible)
            {
                _logger.LogWarning("Attempting to get content from inactive editor");
                return string.Empty;
            }
            return _editorManager.GetEditorText(erdEditor);
        }

        public string GetCurrentFileName()
        {
            return System.IO.Path.GetFileName(currentFilePath ?? "Untitled");
        }

        public void CloseEditor()
        {
            _logger.LogMethodEntry(nameof(CloseEditor), nameof(ErdEditorManager27IM));
            try
            {
                if (erdEditor != null)
                {
                    mainPanel.Controls.Remove(erdEditor);
                    erdEditor.Dispose();
                    erdEditor = null;
                }
                _logger.LogInformation("Editor closed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to close editor", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(CloseEditor), nameof(ErdEditorManager27IM));
            }
        }
    }
}