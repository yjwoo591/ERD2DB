using System;
using System.Windows.Forms;
using System.Drawing;
using ERD2DB.CsBase.CsBase3CB;
using ERD2DB.LibBase.LibBase14LB;
using ERD2DB._25Event;
using ERD2DB.Menu;
using FDCommon.CsBase.CsBase3CB;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase16LB;
using FDCommon.LibBase.LibBase18LB;
using FDCommon.LibBase.LibBase15LB;

namespace ERD2DB.Windows
{
    public partial class Main32W : Form
    {
        private readonly KryptonMenu34ME _menu;
        private readonly CanvasEventHandler25EV _canvasEventHandler;
        private readonly FileOpen25EV _fileOpenHandler;
        private readonly Logging14LB _logger;
        private readonly CanvasLayoutManager16LB _canvasLayoutManager;
        private readonly CanvasService18LB _canvasService;
        private readonly GlobalEventManager15LB _eventManager;

        public Main32W()
        {
            InitializeComponent();
            _logger = Logging14LB.Instance;

            try
            {
                _menu = KryptonMenu34ME.Instance;
                _canvasEventHandler = new CanvasEventHandler25EV(this);
                _fileOpenHandler = new FileOpen25EV(this);
                _canvasService = CanvasService18LB.Instance;
                _eventManager = GlobalEventManager15LB.Instance;
                _canvasLayoutManager = new CanvasLayoutManager16LB(this);

                InitializeMainMenu();
                InitializeEventHandlers();

                this.Resize += Main32W_Resize;
                this.Load += Main32W_Load;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize Main32W: {ex.Message}", ex);
                throw;
            }
        }

        private void InitializeMainMenu()
        {
            try
            {
                _menu.Initialize(this);
                var menuStrip = _menu.CreateMainMenu();
                if (menuStrip != null)
                {
                    this.MainMenuStrip = menuStrip;
                    this.Controls.Add(menuStrip);
                    _logger.LogInformation("Main menu initialized successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize main menu: {ex.Message}", ex);
                throw;
            }
        }

        private void InitializeEventHandlers()
        {
            try
            {
                _eventManager.RegisterEvent<GlobalEventArgs3CB.FileEventArgs>(
                    GlobalEventTypes3CB.FileOpened,
                    async (sender, args) =>
                    {
                        await this.InvokeAsync(() =>
                        {
                            UpdateTitle(args.FilePath);
                        });
                    });

                _logger.LogInformation("Event handlers initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize event handlers: {ex.Message}", ex);
                throw;
            }
        }

        private void Main32W_Load(object sender, EventArgs e)
        {
            try
            {
                _canvasService.Initialize(this);
                _logger.LogInformation("Main window loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Main32W_Load: {ex.Message}", ex);
                MessageBox.Show("프로그램 초기화 중 오류가 발생했습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Main32W_Resize(object sender, EventArgs e)
        {
            _canvasService.HandleResize();
        }

        private void UpdateTitle(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                this.Text = "제목 없음 - ERD2DB";
            }
            else
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                this.Text = $"{fileName} - ERD2DB";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                base.OnFormClosing(e);
                _canvasService.Dispose();
                _logger.LogInformation("Application closing");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during form closing: {ex.Message}", ex);
            }
        }
    }
}