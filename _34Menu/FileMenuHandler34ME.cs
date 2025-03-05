using System;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase16LB;
using FDCommon.LibBase.LibBase18LB;
using FDCommon.LibBase.LibBase15LB;
using FDCommon.CsBase.CsBase3CB;
using ERD2DB._25Event;

namespace ERD2DB.Menu
{
    public class FileMenuHandler34ME
    {
        private readonly Form _mainForm;
        private FileOpen25EV _fileOpen;
        private readonly Logging14LB _logger;
        private readonly GlobalEventManager15LB _eventManager;
        private bool _isInitialized;

        public static FileMenuHandler34ME Instance { get; private set; }

        public FileMenuHandler34ME(Form mainForm)
        {
            _mainForm = mainForm;
            _logger = Logging14LB.Instance;
            _eventManager = GlobalEventManager15LB.Instance;
            Initialize(mainForm);
            Instance = this;
        }

        private void Initialize(Form mainForm)
        {
            if (_isInitialized) return;

            _fileOpen = new FileOpen25EV(mainForm);
            _fileOpen.FileError += OnFileError;
            InitializeEventHandlers();
            _isInitialized = true;
        }

        private void InitializeEventHandlers()
        {
            _eventManager.RegisterEvent<GlobalEventArgs3CB.FileEventArgs>(
                GlobalEventTypes3CB.FileOpened,
                async (sender, args) =>
                {
                    try
                    {
                        // Canvas1에 내용을 표시하기 위한 이벤트 발생
                        await _eventManager.RaiseEventAsync(
                            GlobalEventTypes3CB.CanvasContentUpdated,
                            this,
                            new GlobalEventArgs3CB.CanvasContentEventArgs
                            {
                                CanvasId = "Canvas1",
                                Content = args.Content,
                                Timestamp = DateTime.Now
                            });

                        _logger.LogInformation($"File opened and content loaded: {args.FilePath}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error handling file open event: {ex.Message}", ex);
                        OnFileError(this, ex.Message);
                    }
                });
        }

        private void OnFileError(object sender, string error)
        {
            MessageBox.Show(error, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void HandleFile()
        {
            // Menu root item - no action needed
        }

        public void HandleNew()
        {
            _logger.LogMethodEntry(nameof(HandleNew), nameof(FileMenuHandler34ME));
            try
            {
                _fileOpen.HandleNew();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HandleNew: {ex.Message}", ex);
            }
        }

        public void HandleOpen()
        {
            _logger.LogMethodEntry(nameof(HandleOpen), nameof(FileMenuHandler34ME));
            try
            {
                _fileOpen.HandleOpen();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HandleOpen: {ex.Message}", ex);
            }
        }

        public void HandleSave()
        {
            _logger.LogMethodEntry(nameof(HandleSave), nameof(FileMenuHandler34ME));
            try
            {
                _fileOpen.HandleSave();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HandleSave: {ex.Message}", ex);
            }
        }

        public void HandleSaveAs()
        {
            _logger.LogMethodEntry(nameof(HandleSaveAs), nameof(FileMenuHandler34ME));
            try
            {
                _fileOpen.HandleSave(saveAs: true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HandleSaveAs: {ex.Message}", ex);
            }
        }

        public void HandleExit()
        {
            _logger.LogMethodEntry(nameof(HandleExit), nameof(FileMenuHandler34ME));
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
            }
        }
    }
}