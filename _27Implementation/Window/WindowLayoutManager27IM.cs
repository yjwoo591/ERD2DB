using System;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase16LB;
using FDCommon.CsBase.CsBase4CB;

namespace ERD2DB.Implementation.Window
{
    public class WindowLayoutManager27IM
    {
        private readonly Control mainPanel;
        private readonly EditorManager16LB _editorManager;
        private readonly Logging14LB _logger;
        private readonly KryptonManager14LB _kryptonManager;

        private SplitContainer splitContainer1;  // 전체를 2분할 (1/3 : 2/3)
        private SplitContainer splitContainer2;  // 오른쪽을 다시 2분할 (1/2 : 1/2)
        private Panel erdEditorPanel;    // ERD 에디터 (왼쪽)
        private Panel databasePanel;     // 데이터베이스 관리 (중앙)
        private Panel erdDiagramPanel;   // ERD 다이어그램 (오른쪽)

        public WindowLayoutManager27IM(Control mainPanel)
        {
            this.mainPanel = mainPanel;
            _editorManager = EditorManager16LB.Instance;
            _logger = Logging14LB.Instance;
            _kryptonManager = KryptonManager14LB.Instance;
        }

        public void InitializeLayout()
        {
            _logger.LogMethodEntry(nameof(InitializeLayout), nameof(WindowLayoutManager27IM));
            try
            {
                mainPanel.SuspendLayout();

                // 첫 번째 분할 (전체 화면을 1/3으로 분할)
                splitContainer1 = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    Orientation = Orientation.Vertical,
                    SplitterWidth = 5,
                    SplitterDistance = (int)(mainPanel.Width * 0.33)
                };

                // 두 번째 분할 (나머지 2/3를 다시 반으로 분할)
                splitContainer2 = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    Orientation = Orientation.Vertical,
                    SplitterWidth = 5,
                    SplitterDistance = (int)(mainPanel.Width * 0.33)
                };

                // 패널 초기화
                erdEditorPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = System.Drawing.Color.White,
                    Padding = new Padding(10)
                };

                databasePanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = System.Drawing.Color.White,
                    Padding = new Padding(10)
                };

                erdDiagramPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = System.Drawing.Color.White,
                    Padding = new Padding(10)
                };

                // Split Container 구조 설정
                splitContainer1.Panel1.Controls.Add(erdEditorPanel);    // 1/3 (왼쪽)
                splitContainer2.Panel1.Controls.Add(databasePanel);     // 1/3 (중앙)
                splitContainer2.Panel2.Controls.Add(erdDiagramPanel);   // 1/3 (오른쪽)

                splitContainer1.Panel2.Controls.Add(splitContainer2);
                mainPanel.Controls.Add(splitContainer1);

                // 화면 크기 변경 시 3등분 비율 유지
                mainPanel.Resize += (s, e) =>
                {
                    int oneThird = (int)(mainPanel.Width * 0.33);
                    splitContainer1.SplitterDistance = oneThird;
                    splitContainer2.SplitterDistance = oneThird;
                };

                mainPanel.ResumeLayout(true);
                _logger.LogInformation("Layout initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize layout", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(InitializeLayout), nameof(WindowLayoutManager27IM));
            }
        }

        public Control GetErdEditorPanel()
        {
            return erdEditorPanel;
        }

        public Control GetDatabasePanel()
        {
            return databasePanel;
        }

        public Control GetErdDiagramPanel()
        {
            return erdDiagramPanel;
        }

        public void UpdateLayout()
        {
            _logger.LogMethodEntry(nameof(UpdateLayout), nameof(WindowLayoutManager27IM));
            try
            {
                mainPanel.SuspendLayout();

                // 현재 분할 비율 유지하면서 컨트롤 새로고침
                splitContainer1.SuspendLayout();
                splitContainer2.SuspendLayout();

                erdEditorPanel.Refresh();
                databasePanel.Refresh();
                erdDiagramPanel.Refresh();

                splitContainer2.ResumeLayout();
                splitContainer1.ResumeLayout();
                mainPanel.ResumeLayout(true);

                _logger.LogInformation("Layout updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update layout", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(UpdateLayout), nameof(WindowLayoutManager27IM));
            }
        }

        public void ClearLayout()
        {
            _logger.LogMethodEntry(nameof(ClearLayout), nameof(WindowLayoutManager27IM));
            try
            {
                erdEditorPanel.Controls.Clear();
                databasePanel.Controls.Clear();
                erdDiagramPanel.Controls.Clear();
                _logger.LogInformation("Layout cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to clear layout", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(ClearLayout), nameof(WindowLayoutManager27IM));
            }
        }

        public void Dispose()
        {
            _logger.LogMethodEntry(nameof(Dispose), nameof(WindowLayoutManager27IM));
            try
            {
                splitContainer1?.Dispose();
                splitContainer2?.Dispose();
                erdEditorPanel?.Dispose();
                databasePanel?.Dispose();
                erdDiagramPanel?.Dispose();
                _logger.LogInformation("Layout resources disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to dispose layout resources", ex);
                throw;
            }
            finally
            {
                _logger.LogMethodExit(nameof(Dispose), nameof(WindowLayoutManager27IM));
            }
        }
    }
}