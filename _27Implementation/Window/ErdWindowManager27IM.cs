using System;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;

namespace ERD2DB.Implementation.Window
{
    public class ErdWindowManager27IM
    {
        private readonly Form _mainForm;
        private readonly Logging14LB _logger;
        private Control _mainPanel;
        private string _currentFilePath;
        private readonly MermaidJs14LB _mermaidJs;

        public ErdWindowManager27IM(Form mainForm)
        {
            _mainForm = mainForm;
            _logger = Logging14LB.Instance;
            _mermaidJs = MermaidJs14LB.Instance;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _mainPanel = new Panel();
            _mainPanel.Dock = DockStyle.Fill;

            var menuHeight = 0;
            foreach (Control control in _mainForm.Controls)
            {
                if (control is MenuStrip)
                {
                    menuHeight = control.Height;
                    break;
                }
            }

            _mainPanel.Location = new System.Drawing.Point(0, menuHeight);
            _mainPanel.Height = _mainForm.ClientSize.Height - menuHeight;
            _mainPanel.BackColor = System.Drawing.Color.White;
            _mainForm.Controls.Add(_mainPanel);
        }

        public async void OpenFile(string filePath)
        {
            _logger.LogMethodEntry(nameof(OpenFile), nameof(ErdWindowManager27IM));
            try
            {
                if (!ValidateErdFile(filePath))
                {
                    return;
                }

                string content = System.IO.File.ReadAllText(filePath);

                var validationResult = await _mermaidJs.ValidateMermaidAsync(content);
                if (!validationResult.Success)
                {
                    MessageBox.Show($"ERD 파일 검증 실패: {validationResult.ErrorMessage}",
                        "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _currentFilePath = filePath;
                InitializeLayout();
                LoadFileContent(content);
                _logger.LogInformation($"File opened successfully: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to open file: {filePath}", ex);
                MessageBox.Show($"파일을 여는 중 오류가 발생했습니다: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _logger.LogMethodExit(nameof(OpenFile), nameof(ErdWindowManager27IM));
            }
        }

        private bool ValidateErdFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogError("File path is null or empty");
                MessageBox.Show("파일 경로가 유효하지 않습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError($"File does not exist: {filePath}");
                MessageBox.Show("파일이 존재하지 않습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            string extension = System.IO.Path.GetExtension(filePath).ToLower();
            if (extension != ".mermaid" && extension != ".mmd" && extension != ".erd")
            {
                _logger.LogError($"Invalid file extension: {extension}");
                MessageBox.Show("지원되지 않는 파일 형식입니다. (.mermaid, .mmd 또는 .erd 파일만 가능)", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void InitializeLayout()
        {
            _logger.LogMethodEntry(nameof(InitializeLayout), nameof(ErdWindowManager27IM));
            try
            {
                _mainPanel.Controls.Clear();
                var label = new Label
                {
                    Text = "ERD Content will be displayed here",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                _mainPanel.Controls.Add(label);
                _mainPanel.Refresh();

                _logger.LogInformation("Layout initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize layout", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(InitializeLayout), nameof(ErdWindowManager27IM));
            }
        }

        private void LoadFileContent(string content)
        {
            _logger.LogMethodEntry(nameof(LoadFileContent), nameof(ErdWindowManager27IM));
            try
            {
                if (!string.IsNullOrWhiteSpace(content))
                {
                    if (_mainPanel.Controls.Count > 0 && _mainPanel.Controls[0] is Label label)
                    {
                        label.Text = content;
                    }
                }
                _logger.LogInformation("File content loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load file content", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(LoadFileContent), nameof(ErdWindowManager27IM));
            }
        }

        public string GetCurrentFileName()
        {
            return System.IO.Path.GetFileName(_currentFilePath ?? "Untitled");
        }

        public void CloseWindow()
        {
            _logger.LogMethodEntry(nameof(CloseWindow), nameof(ErdWindowManager27IM));
            try
            {
                _mainPanel.Controls.Clear();
                _currentFilePath = null;
                _logger.LogInformation("Window closed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to close window", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(CloseWindow), nameof(ErdWindowManager27IM));
            }
        }

        public void Dispose()
        {
            _logger.LogMethodEntry(nameof(Dispose), nameof(ErdWindowManager27IM));
            try
            {
                _mainPanel?.Dispose();
                _logger.LogInformation("Resources disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to dispose resources", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(Dispose), nameof(ErdWindowManager27IM));
            }
        }
    }
}