using System;
using System.Windows.Forms;
using System.Drawing;
using FDCommon.LibBase.LibBase14LB;
using FastColoredTextBoxNS;
using FDCommon.CsBase.CsBase4CB;

namespace FDCommon.LibBase.LibBase16LB
{
    public class EditorManager16LB
    {
        private static EditorManager16LB _instance;
        private readonly KryptonManager14LB _kryptonManager;
        private readonly CodeEditor14LB _codeEditor;
        private readonly Logging14LB _logger;
        private string _currentFilePath;

        public static EditorManager16LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EditorManager16LB();
                }
                return _instance;
            }
        }

        private EditorManager16LB()
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
                var editor = _codeEditor.EditorControl as FastColoredTextBox;
                if (editor == null) return;

                if (editor.IsChanged)
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
                        editor.Text = content;
                        _currentFilePath = dialog.FileName;
                        editor.ClearUndo();
                        editor.IsChanged = false;
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
                var editor = _codeEditor.EditorControl as FastColoredTextBox;
                if (editor == null) return;

                if (string.IsNullOrEmpty(_currentFilePath))
                {
                    SaveFileAs();
                    return;
                }

                System.IO.File.WriteAllText(_currentFilePath, editor.Text);
                editor.IsChanged = false;
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
                var editor = _codeEditor.EditorControl as FastColoredTextBox;
                if (editor == null) return;

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
                        System.IO.File.WriteAllText(dialog.FileName, editor.Text);
                        _currentFilePath = dialog.FileName;
                        editor.IsChanged = false;
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
                    if (editor is FastColoredTextBox textBox)
                    {
                        textBox.Text = text;
                        _logger.LogInformation($"Set editor text, length: {text.Length}");
                    }
                    else
                    {
                        _logger.LogError($"Editor is not FastColoredTextBox, actual type: {editor.GetType().Name}");
                        throw new InvalidOperationException("Invalid editor type");
                    }
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
                if (editor is FastColoredTextBox textBox)
                {
                    return textBox.Text;
                }
                _logger.LogError($"Editor is not FastColoredTextBox, actual type: {editor.GetType().Name}");
                throw new InvalidOperationException("Invalid editor type");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get editor text: {ex.Message}", ex);
                throw;
            }
        }

        public void RenderDiagram(Control editor, ErdData4CB erdData)
        {
            try
            {
                if (editor == null)
                {
                    _logger.LogWarning("Editor is null for rendering diagram");
                    return;
                }

                if (erdData == null)
                {
                    _logger.LogWarning("ERD data is null for rendering");
                    return;
                }

                if (editor is FastColoredTextBox textBox)
                {
                    var mermaidCode = new System.Text.StringBuilder();
                    mermaidCode.AppendLine("erDiagram");

                    foreach (var entity in erdData.Entities)
                    {
                        mermaidCode.AppendLine($"    {entity.Name} {{");
                        foreach (var field in entity.Fields)
                        {
                            string fieldLine = $"        {field.Type} {field.Name}";
                            if (field.IsPrimaryKey) fieldLine += " PK";
                            if (field.IsForeignKey) fieldLine += " FK";
                            mermaidCode.AppendLine(fieldLine);
                        }
                        mermaidCode.AppendLine("    }");
                    }

                    foreach (var relation in erdData.Relations)
                    {
                        mermaidCode.AppendLine($"    {relation.FromEntity} {relation.RelationType} {relation.ToEntity}");
                    }

                    textBox.Text = mermaidCode.ToString();
                    textBox.ClearUndo();
                    textBox.IsChanged = false;
                    _logger.LogInformation("Diagram rendered successfully");
                }
                else
                {
                    _logger.LogError($"Editor is not FastColoredTextBox, actual type: {editor.GetType().Name}");
                    throw new InvalidOperationException("Invalid editor type");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to render diagram: {ex.Message}", ex);
                throw;
            }
        }

        public ToolStripStatusLabel GetFontSizeLabel()
        {
            return _codeEditor.GetFontSizeLabel();
        }
    }
}