using System;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase15LB;
using FDCommon.CsBase.CsBase3CB;
using ERD2DB._23AsyncTask;

namespace ERD2DB._25Event
{
    public class FileOpen25EV
    {
        private readonly Form _mainForm;
        private readonly Logging14LB _logger;
        private readonly GlobalEventManager15LB _eventManager;
        private readonly CanvasMonitorTask23AT _canvasMonitor;

        public event EventHandler<string> FileError;

        public FileOpen25EV(Form mainForm)
        {
            _mainForm = mainForm;
            _logger = Logging14LB.Instance;
            _eventManager = GlobalEventManager15LB.Instance;
            _canvasMonitor = CanvasMonitorTask23AT.Instance;
        }

        public void HandleOpen()
        {
            try
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = "ERD Files|*.mermaid;*.mmd;*.erd|All Files|*.*";
                    dialog.Title = "ERD 파일 열기";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _logger.LogInformation($"Selected file: {dialog.FileName}");

                        string content = System.IO.File.ReadAllText(dialog.FileName);

                        // 파일 열기 이벤트 발생
                        _eventManager.RaiseEventAsync(
                            GlobalEventTypes3CB.FileOpened,
                            this,
                            new GlobalEventArgs3CB.FileEventArgs
                            {
                                FilePath = dialog.FileName,
                                Content = content,
                                Timestamp = DateTime.Now
                            });

                        // Canvas 내용 업데이트 이벤트 발생
                        _eventManager.RaiseEventAsync(
                            GlobalEventTypes3CB.CanvasContentUpdated,
                            this,
                            new GlobalEventArgs3CB.CanvasContentEventArgs
                            {
                                CanvasId = "Canvas1",
                                Content = content,
                                Timestamp = DateTime.Now
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to open file: {ex.Message}", ex);
                FileError?.Invoke(this, $"파일 오픈 실패: {ex.Message}");
            }
        }

        public void HandleNew()
        {
            try
            {
                _eventManager.RaiseEventAsync(
                    GlobalEventTypes3CB.ContentChanged,
                    this,
                    new GlobalEventArgs3CB.FileEventArgs
                    {
                        FilePath = null,
                        Content = string.Empty,
                        Timestamp = DateTime.Now
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create new file: {ex.Message}", ex);
                FileError?.Invoke(this, $"새 파일 생성 실패: {ex.Message}");
            }
        }

        public void HandleSave(bool saveAs = false)
        {
            try
            {
                _eventManager.RaiseEventAsync(
                    GlobalEventTypes3CB.FileSaved,
                    this,
                    new GlobalEventArgs3CB.FileEventArgs
                    {
                        Timestamp = DateTime.Now
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save file: {ex.Message}", ex);
                FileError?.Invoke(this, $"파일 저장 실패: {ex.Message}");
            }
        }
    }
}