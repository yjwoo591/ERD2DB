using System;
using System.Windows.Forms;
using System.Drawing;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase16LB
{
    public class CanvasLayoutUI16LB : CanvasLayoutBase16LB
    {
        protected Panel _leftPanel;
        protected Panel _centerPanel;
        protected Panel _rightPanel;
        protected const int TITLE_HEIGHT = 25;

        public Panel LeftPanel => _leftPanel;
        public Panel CenterPanel => _centerPanel;
        public Panel RightPanel => _rightPanel;

        public override void Initialize(Control parentControl)
        {
            try
            {
                InitializePanels();
                base.Initialize(parentControl);
                AttachPanelsToSplitContainers();
                _logger.LogInformation("Canvas UI initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize canvas UI: {ex.Message}", ex);
                throw;
            }
        }

        protected virtual void InitializePanels()
        {
            _leftPanel = CreatePanel();
            _centerPanel = CreatePanel();
            _rightPanel = CreatePanel();

            AddTitleLabel(_leftPanel, "ERD Editor");
            AddTitleLabel(_centerPanel, "Database Editor");
            AddTitleLabel(_rightPanel, "ERD Diagram");
        }

        protected virtual Panel CreatePanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
        }

        protected virtual void AddTitleLabel(Panel panel, string title)
        {
            var label = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = TITLE_HEIGHT,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Padding = new Padding(5)
            };
            panel.Controls.Add(label);
        }

        protected virtual void AttachPanelsToSplitContainers()
        {
            _splitContainer1.AddToPanel1(_leftPanel);
            _splitContainer2.AddToPanel1(_centerPanel);
            _splitContainer2.AddToPanel2(_rightPanel);
        }

        public virtual void ShowPanel(int panelNumber, bool show)
        {
            try
            {
                switch (panelNumber)
                {
                    case 1:
                        _leftPanel.Visible = show;
                        break;
                    case 2:
                        _centerPanel.Visible = show;
                        break;
                    case 3:
                        _rightPanel.Visible = show;
                        break;
                }

                _logger.LogInformation($"Panel {panelNumber} visibility set to {show}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to set panel visibility: {ex.Message}", ex);
            }
        }

        public virtual bool IsPanelVisible(int panelNumber)
        {
            return panelNumber switch
            {
                1 => _leftPanel?.Visible ?? false,
                2 => _centerPanel?.Visible ?? false,
                3 => _rightPanel?.Visible ?? false,
                _ => false
            };
        }

        public override void Dispose()
        {
            _leftPanel?.Dispose();
            _centerPanel?.Dispose();
            _rightPanel?.Dispose();
            base.Dispose();
        }
    }
}