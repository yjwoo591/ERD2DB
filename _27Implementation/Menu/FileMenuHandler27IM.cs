using System;
using System.Windows.Forms;
using System.Linq;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase16LB;
using FDCommon.LibBase.LibBase17LB;

namespace ERD2DB.Implementation
{
    public class FileMenuHandler27IM
    {
        private readonly EditorManager16LB _editorManager;
        private readonly Form _mainForm;
        private readonly Logging14LB _logger;
        private readonly BaseWindowManager17LB _windowManager;

        public FileMenuHandler27IM()
        {
            _editorManager = EditorManager16LB.Instance;
            _logger = Logging14LB.Instance;
            _mainForm = Application.OpenForms[0];
            _windowManager = new BaseWindowManager17LB();
        }

        public void HandleOpen()
        {
            _logger.LogMethodEntry(nameof(HandleOpen), nameof(FileMenuHandler27IM));
            try
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "ERD Files|*.mermaid;*.mmd;*.erd|All Files|*.*";
                    dialog.Title = "Open ERD File";
                    dialog.CheckFileExists = true;
                    dialog.CheckPathExists = true;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string fileName = dialog.FileName;
                        _logger.LogInformation($"Selected file: {fileName}");

                        try
                        {
                            string content = System.IO.File.ReadAllText(fileName);
                            _logger.LogInformation($"File content read successfully. Length: {content.Length}");

                            var splitContainer1 = FindSplitContainer(_mainForm);
                            if (splitContainer1 == null)
                            {
                                throw new InvalidOperationException("Split Container를 찾을 수 없습니다.");
                            }

                            Control erdEditorPanel = splitContainer1.Panel1.Controls[0];
                            if (erdEditorPanel == null)
                            {
                                throw new InvalidOperationException("ERD Editor 패널을 찾을 수 없습니다.");
                            }

                            erdEditorPanel.Controls.Clear();

                            var editor = _editorManager.CreateCodeEditor();
                            if (editor == null)
                            {
                                throw new InvalidOperationException("코드 에디터를 생성할 수 없습니다.");
                            }

                            _editorManager.SetEditorText(editor, content);
                            editor.Dock = DockStyle.Fill;
                            erdEditorPanel.Controls.Add(editor);

                            // 텍스트 길이에 맞춰 패널 폭 조정
                            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            if (lines.Length > 0)
                            {
                                _windowManager.SetResizeRequired();
                                _windowManager.AdjustPanelWidth(content, BaseWindowManager17LB.ResizeReason.FileOpen);
                                _logger.LogInformation("Window resize flag set after loading new file");
                            }

                            string displayName = System.IO.Path.GetFileName(fileName);
                            _mainForm.Text = $"{displayName} - ERD2DB [{_mainForm.Location.X},{_mainForm.Location.Y}][{_mainForm.Width}x{_mainForm.Height}]";

                            editor.Focus();
                            _logger.LogInformation("Editor created and content loaded successfully");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing file: {ex.Message}", ex);
                            MessageBox.Show($"파일을 처리하는 중 오류가 발생했습니다.\n{ex.Message}",
                                "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HandleOpen: {ex.Message}", ex);
                MessageBox.Show($"파일을 여는 중 오류가 발생했습니다.\n{ex.Message}",
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _logger.LogMethodExit(nameof(HandleOpen), nameof(FileMenuHandler27IM));
            }
        }

        private SplitContainer FindSplitContainer(Control control)
        {
            if (control is SplitContainer splitContainer)
            {
                return splitContainer;
            }

            foreach (Control child in control.Controls)
            {
                var result = FindSplitContainer(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public void HandleNew()
        {
            _logger.LogMethodEntry(nameof(HandleNew), nameof(FileMenuHandler27IM));
            try
            {
                var splitContainer1 = FindSplitContainer(_mainForm);
                if (splitContainer1 == null)
                {
                    throw new InvalidOperationException("Split Container를 찾을 수 없습니다.");
                }

                Control erdEditorPanel = splitContainer1.Panel1.Controls[0];
                if (erdEditorPanel == null)
                {
                    throw new InvalidOperationException("ERD Editor 패널을 찾을 수 없습니다.");
                }

                erdEditorPanel.Controls.Clear();

                var editor = _editorManager.CreateCodeEditor();
                if (editor == null)
                {
                    throw new InvalidOperationException("코드 에디터를 생성할 수 없습니다.");
                }

                editor.Dock = DockStyle.Fill;
                erdEditorPanel.Controls.Add(editor);

                _windowManager.SetResizeRequired();
                _logger.LogInformation("Window resize flag set after creating new file");

                _mainForm.Text = $"제목 없음 - ERD2DB [{_mainForm.Location.X},{_mainForm.Location.Y}][{_mainForm.Width}x{_mainForm.Height}]";

                editor.Focus();
                _logger.LogInformation("New editor created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HandleNew: {ex.Message}", ex);
                MessageBox.Show("새 파일을 생성하는 중 오류가 발생했습니다.",
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _logger.LogMethodExit(nameof(HandleNew), nameof(FileMenuHandler27IM));
            }
        }

        public void HandleSave()
        {
            _logger.LogMethodEntry(nameof(HandleSave), nameof(FileMenuHandler27IM));
            try
            {
                var splitContainer1 = FindSplitContainer(_mainForm);
                if (splitContainer1 == null)
                {
                    throw new InvalidOperationException("Split Container를 찾을 수 없습니다.");
                }

                Control erdEditorPanel = splitContainer1.Panel1.Controls[0];
                if (erdEditorPanel == null || erdEditorPanel.Controls.Count == 0)
                {
                    _logger.LogWarning("No active editor to save");
                    return;
                }

                var editor = erdEditorPanel.Controls[0];
                var content = _editorManager.GetEditorText(editor);

                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = "ERD Files|*.mermaid;*.mmd;*.erd|All Files|*.*";
                    dialog.Title = "Save ERD File";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        System.IO.File.WriteAllText(dialog.FileName, content);
                        _logger.LogInformation($"File saved successfully: {dialog.FileName}");

                        string displayName = System.IO.Path.GetFileName(dialog.FileName);
                        _mainForm.Text = $"{displayName} - ERD2DB [{_mainForm.Location.X},{_mainForm.Location.Y}][{_mainForm.Width}x{_mainForm.Height}]";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HandleSave: {ex.Message}", ex);
                MessageBox.Show("파일을 저장하는 중 오류가 발생했습니다.",
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _logger.LogMethodExit(nameof(HandleSave), nameof(FileMenuHandler27IM));
            }
        }

        public void HandleSaveAs()
        {
            HandleSave();
        }

        public void HandleExit()
        {
            _logger.LogMethodEntry(nameof(HandleExit), nameof(FileMenuHandler27IM));
            try
            {
                if (MessageBox.Show("프로그램을 종료하시겠습니까?", "종료 확인",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _logger.LogInformation("User confirmed application exit");
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HandleExit: {ex.Message}", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(HandleExit), nameof(FileMenuHandler27IM));
            }
        }
    }
}