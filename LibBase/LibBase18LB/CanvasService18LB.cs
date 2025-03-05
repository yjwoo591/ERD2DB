using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using FDCommon.LibBase.LibBase12LB;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase16LB;
using FDCommon.LibBase.LibBase17LB;

namespace FDCommon.LibBase.LibBase18LB
{
    public class CanvasService18LB
    {
        private static CanvasService18LB _instance;
        private readonly Logging14LB _logger;
        private readonly KryptonManager14LB _kryptonManager;
        private readonly CanvasMonitorBase17LB _canvasMonitor;
        private CanvasLayoutManager16LB _layoutManager;
        private bool _isInitialized;
        private Panel _canvas0;
        private SplitContainer _splitContainer1;
        private SplitContainer _splitContainer2;
        private Panel[] _canvasPanels;
        private readonly string[] _canvasTitles = { "ERD Editor", "Database Editor", "ERD Diagram" };

        public static CanvasService18LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CanvasService18LB();
                }
                return _instance;
            }
        }

        private CanvasService18LB()
        {
            _logger = Logging14LB.Instance;
            _kryptonManager = KryptonManager14LB.Instance;
            _canvasMonitor = new CanvasMonitorBase17LB();
            _canvasPanels = new Panel[3];
        }

        public void Initialize(Form mainForm)
        {
            if (_isInitialized) return;

            try
            {
                _logger.LogMethodEntry(nameof(Initialize), nameof(CanvasService18LB));

                // Canvas0 초기화
                InitializeCanvas0(mainForm);

                // LayoutManager 초기화
                _layoutManager = new CanvasLayoutManager16LB(_canvas0);

                // Split Container 초기화
                InitializeSplitContainers();

                // Canvas1-3 초기화
                InitializeCanvasPanels();

                _isInitialized = true;
                _logger.LogInformation("Canvas service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize canvas service: {ex.Message}", ex);
                throw;
            }
        }

        private void InitializeCanvas0(Form mainForm)
        {
            int menuHeight = 0;
            foreach (Control control in mainForm.Controls)
            {
                if (control is MenuStrip)
                {
                    menuHeight = control.Height;
                    break;
                }
            }

            _canvas0 = new Panel
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, menuHeight),
                Size = new Size(mainForm.ClientSize.Width, mainForm.ClientSize.Height - menuHeight),
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            mainForm.Controls.Add(_canvas0);
            _canvas0.BringToFront();
        }

        private void InitializeSplitContainers()
        {
            _splitContainer1 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 5,
                Panel1MinSize = 100,
                Panel2MinSize = 100
            };

            _splitContainer2 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 5,
                Panel1MinSize = 100,
                Panel2MinSize = 100
            };

            _splitContainer1.Panel2.Controls.Add(_splitContainer2);
            _canvas0.Controls.Add(_splitContainer1);

            // 3등분 설정
            _splitContainer1.SplitterDistance = _canvas0.Width / 3;
            _splitContainer2.SplitterDistance = _splitContainer2.Width / 2;

            // SplitterMoved 이벤트 처리
            _splitContainer1.SplitterMoved += SplitContainer_SplitterMoved;
            _splitContainer2.SplitterMoved += SplitContainer_SplitterMoved;
        }

        private void InitializeCanvasPanels()
        {
            for (int i = 0; i < 3; i++)
            {
                _canvasPanels[i] = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(5)
                };

                // 제목 레이블 추가
                var titleLabel = new Label
                {
                    Text = _canvasTitles[i],
                    Dock = DockStyle.Top,
                    Height = 25,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    BackColor = Color.FromArgb(240, 240, 240),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(5, 0, 0, 0)
                };

                _canvasPanels[i].Controls.Add(titleLabel);

                // 공통 CanvasMonitor 사용
                _canvasMonitor.SetCanvasPanel(i, _canvasPanels[i]);

                // 패널을 적절한 SplitContainer에 추가
                if (i == 0)
                    _splitContainer1.Panel1.Controls.Add(_canvasPanels[i]);
                else if (i == 1)
                    _splitContainer2.Panel1.Controls.Add(_canvasPanels[i]);
                else
                    _splitContainer2.Panel2.Controls.Add(_canvasPanels[i]);

                // 테두리 추가
                _canvasPanels[i].Paint += (sender, e) =>
                {
                    var panel = (Panel)sender;
                    using (var pen = new Pen(Color.LightGray))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                    }
                };
            }
        }

        private void SplitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            // 분할 비율 조정 시 처리할 로직
            if (sender == _splitContainer1)
            {
                // 첫 번째 분할선이 이동된 경우
                _splitContainer2.SplitterDistance = _splitContainer2.Width / 2;
            }
        }

        public void HandleResize()
        {
            if (!_isInitialized) return;

            try
            {
                // Canvas0 크기 조정
                if (_canvas0.Parent is Form mainForm)
                {
                    int menuHeight = mainForm.Controls.OfType<MenuStrip>().FirstOrDefault()?.Height ?? 0;
                    _canvas0.Size = new Size(mainForm.ClientSize.Width, mainForm.ClientSize.Height - menuHeight);
                }

                // 분할 비율 유지
                _splitContainer1.SplitterDistance = _canvas0.Width / 3;
                _splitContainer2.SplitterDistance = _splitContainer2.Width / 2;

                _logger.LogInformation("Canvas layout resized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to handle resize: {ex.Message}", ex);
            }
        }

        public Panel GetCanvas(int index)
        {
            if (index >= 0 && index < _canvasPanels.Length)
            {
                return _canvasPanels[index];
            }
            return null;
        }

        public void SetCanvasPanel(int index, Panel panel)
        {
            if (index >= 0 && index < _canvasPanels.Length)
            {
                _canvasPanels[index] = panel;
                _canvasMonitor.SetCanvasPanel(index, panel);
            }
        }

        public void Dispose()
        {
            try
            {
                _splitContainer1?.Dispose();
                _splitContainer2?.Dispose();
                foreach (var panel in _canvasPanels)
                {
                    panel?.Dispose();
                }
                _canvas0?.Dispose();
                _logger.LogInformation("Canvas service disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during canvas service disposal: {ex.Message}", ex);
            }
        }
    }
}