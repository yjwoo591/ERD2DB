using System;
using System.Windows.Forms;
using System.Drawing;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase16LB
{
    public class CodeEditorManager16LB
    {
        private static CodeEditorManager16LB _instance;
        private readonly KryptonManager14LB _kryptonManager;
        private readonly CodeEditor14LB _codeEditor;
        private readonly Logging14LB _logger;
        private string _currentFilePath;

        public static CodeEditorManager16LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CodeEditorManager16LB();
                }
                return _instance;
            }
        }

        protected CodeEditorManager16LB()
        {
            _logger = Logging14LB.Instance;
            _kryptonManager = KryptonManager14LB.Instance;
            _codeEditor = new CodeEditor14LB(_logger);
            _currentFilePath = null;
        }

        public void OpenFile()
        {
            try
            {
                var editor = _codeEditor.EditorControl;
                if (editor == null) return;

                if (_codeEditor.Text != string.Empty)
                {
                    var result = MessageBox.Show(
                        "현재 파일이 수정되었습니다. 저장하시겠습니까?",
                        "저장 확인",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        SaveFile();
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        return;
                    }
                }

                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "ERD Files|*.mermaid;*.mmd;*.erd|All Files|*.*";
                    dialog.Title = "Open ERD File";
                    dialog.CheckFileExists = true;
                    dialog.CheckPathExists = true;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (dialog.FileName == _currentFilePath)
                        {
                            MessageBox.Show("파일이 이미 열려있습니다.",
                                "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        string content = System.IO.File.ReadAllText(dialog.FileName);
                        _codeEditor.Text = content;
                        _currentFilePath = dialog.FileName;
                        _logger.LogInformation($"File opened successfully: {_currentFilePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to open file: {ex.Message}", ex);
                MessageBox.Show($"파일을 여는 중 오류가 발생했습니다.\n{ex.Message}",
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SaveFile()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentFilePath))
                {
                    SaveFileAs();
                    return;
                }

                System.IO.File.WriteAllText(_currentFilePath, _codeEditor.Text);
                _logger.LogInformation($"File saved successfully: {_currentFilePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save file: {ex.Message}", ex);
                MessageBox.Show($"파일을 저장하는 중 오류가 발생했습니다.\n{ex.Message}",
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SaveFileAs()
        {
            try
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = "ERD Files|*.mermaid;*.mmd;*.erd|All Files|*.*";
                    dialog.Title = "Save ERD File";

                    if (!string.IsNullOrEmpty(_currentFilePath))
                    {
                        dialog.FileName = System.IO.Path.GetFileName(_currentFilePath);
                        dialog.InitialDirectory = System.IO.Path.GetDirectoryName(_currentFilePath);
                    }

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        System.IO.File.WriteAllText(dialog.FileName, _codeEditor.Text);
                        _currentFilePath = dialog.FileName;
                        _logger.LogInformation($"File saved successfully: {_currentFilePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save file: {ex.Message}", ex);
                MessageBox.Show($"파일을 저장하는 중 오류가 발생했습니다.\n{ex.Message}",
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public Control CreateCodeEditor()
        {
            try
            {
                var editor = _codeEditor.EditorControl;
                editor.Dock = DockStyle.Fill;
                _logger.LogInformation("Created code editor");
                return editor;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to create code editor", ex);
                throw;
            }
        }

        public void SetEditorText(Control editor, string text)
        {
            try
            {
                if (editor != null && text != null)
                {
                    _codeEditor.Text = text;
                    _logger.LogInformation($"Set editor text, length: {text.Length}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to set editor text: {ex.Message}", ex);
                throw;
            }
        }

        public string GetEditorText(Control editor)
        {
            try
            {
                return _codeEditor.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get editor text: {ex.Message}", ex);
                throw;
            }
        }

        public ToolStripStatusLabel GetFontSizeLabel()
        {
            return _codeEditor.GetFontSizeLabel();
        }
    }
}