using System;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase16LB;

namespace FDCommon.LibBase.LibBase17LB
{
    public class BaseMenuManager17LB
    {
        protected readonly CodeEditor14LB CodeEditor;
        protected readonly Logging14LB Logger;
        protected readonly BaseWindowManager17LB WindowManager;
        protected readonly Form MainForm;
        protected string CurrentFilePath;

        public BaseMenuManager17LB(Form mainForm)
        {
            MainForm = mainForm;
            Logger = Logging14LB.Instance;
            CodeEditor = new CodeEditor14LB(Logger);
            WindowManager = new BaseWindowManager17LB();
            InitializeComponents();
        }

        protected virtual void InitializeComponents()
        {
            try
            {
                if (MainForm != null)
                {
                    foreach (Control control in MainForm.Controls)
                    {
                        if (control is Panel contentPanel)
                        {
                            WindowManager.InitializeSplitContainers(contentPanel);
                            Logger.LogInformation("Window manager initialized successfully");
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to initialize components", ex);
                throw;
            }
        }

        public virtual void UpdateEditor(Control editor, string content)
        {
            Logger.LogMethodEntry(nameof(UpdateEditor), nameof(BaseMenuManager17LB));
            try
            {
                if (editor == null)
                {
                    throw new ArgumentNullException(nameof(editor));
                }

                // 텍스트 설정
                CodeEditor.SetEditorText(editor, content);
                editor.Dock = DockStyle.Fill;

                // 왼쪽 패널에 에디터 추가
                var leftPanel = WindowManager.LeftPanel;
                if (leftPanel == null)
                {
                    throw new InvalidOperationException("Left panel is not initialized");
                }

                leftPanel.SuspendLayout();
                leftPanel.Controls.Clear();
                leftPanel.Controls.Add(editor);
                leftPanel.ResumeLayout(true);

                // 에디터에 포커스
                editor.Focus();

                // 패널 크기 조정
                WindowManager.SetResizeRequired();
                WindowManager.AdjustPanelWidth(content, BaseWindowManager17LB.ResizeReason.FileOpen);

                Logger.LogInformation($"Editor updated successfully with content length: {content?.Length ?? 0}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to update editor: {ex.Message}", ex);
                throw;
            }
            finally
            {
                Logger.LogMethodExit(nameof(UpdateEditor), nameof(BaseMenuManager17LB));
            }
        }

        public virtual void UpdateFormTitle(string fileName = null)
        {
            try
            {
                string displayName = fileName != null ? System.IO.Path.GetFileName(fileName) : "제목 없음";
                MainForm.Text = $"{displayName} - ERD2DB [{MainForm.Location.X},{MainForm.Location.Y}][{MainForm.Width}x{MainForm.Height}]";
                Logger.LogInformation($"Form title updated: {MainForm.Text}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to update form title: {ex.Message}", ex);
            }
        }

        public virtual Control GetLeftPanel()
        {
            return WindowManager?.LeftPanel;
        }

        public virtual Control CreateEditor()
        {
            try
            {
                var editor = CodeEditor.EditorControl;
                if (editor == null)
                {
                    throw new InvalidOperationException("Failed to create editor control");
                }
                Logger.LogInformation("Editor control created successfully");
                return editor;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to create editor: {ex.Message}", ex);
                throw;
            }
        }

        public virtual string GetContent()
        {
            try
            {
                var leftPanel = WindowManager?.LeftPanel;
                if (leftPanel?.Controls.Count > 0)
                {
                    return CodeEditor.GetEditorText(leftPanel.Controls[0]);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to get content: {ex.Message}", ex);
                throw;
            }
        }
    }
}