using System;
using System.Windows.Forms;
using System.Drawing;
using FDCommon.LibBase.LibBase13LB;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase16LB;
using FDCommon.LibBase.LibBase19LB;
using ERD2DB.Implementation.ERD;
using FastColoredTextBoxNS;

namespace ERD2DB.Implementation.Menu
{
    public class ViewMenuHandler27IM : MermaidParser19LB
    {
        private readonly Form _mainForm;
        private readonly Logging14LB _logger;
        private readonly IErdHandler13LB _erdHandler;
        private readonly EditorManager16LB _editorManager;
        private Panel _erdDiagramPanel;
        private Control _erdViewer;
        private Label _initialMessageLabel;
        private bool _isErdContentValid = false;
        private string _lastValidContent = string.Empty;

        public ViewMenuHandler27IM()
        {
            _mainForm = Application.OpenForms[0];
            _logger = Logging14LB.Instance;
            _erdHandler = new ErdDiagram21I();
            _editorManager = EditorManager16LB.Instance;
            InitializeErdPanel();
        }

        private void InitializeErdPanel()
        {
            try
            {
                _erdDiagramPanel = FindErdDiagramPanel(_mainForm);
                if (_erdDiagramPanel == null)
                {
                    _logger.LogError("ERD Diagram panel not found");
                    return;
                }

                _initialMessageLabel = new Label
                {
                    Text = "ERD 파일을 열고 View -> ERD를 클릭하세요",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = false
                };

                _erdViewer = (_erdHandler as ErdDiagram21I)?.GetViewer();
                if (_erdViewer != null)
                {
                    _erdViewer.Dock = DockStyle.Fill;
                    _erdViewer.Visible = false;
                    _erdViewer.MouseWheel += (s, e) =>
                    {
                        if (Control.ModifierKeys == Keys.Control)
                        {
                            _erdHandler.UpdateZoom(e.Delta > 0);
                        }
                    };

                    _erdDiagramPanel.Controls.Clear();
                    _erdDiagramPanel.Controls.Add(_erdViewer);
                    _erdDiagramPanel.Controls.Add(_initialMessageLabel);

                    _initialMessageLabel.BringToFront();
                }

                _logger.LogInformation("ERD panel initialized with initial message");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize ERD panel", ex);
            }
        }

        private Panel FindErdDiagramPanel(Control control)
        {
            if (control is SplitContainer splitContainer1)
            {
                if (splitContainer1.Panel2.Controls.Count > 0 &&
                    splitContainer1.Panel2.Controls[0] is SplitContainer splitContainer2)
                {
                    return splitContainer2.Panel2.Controls[0] as Panel;
                }
            }

            foreach (Control child in control.Controls)
            {
                var result = FindErdDiagramPanel(child);
                if (result != null) return result;
            }
            return null;
        }

        private Panel FindErdEditor(Control control)
        {
            if (control is SplitContainer splitContainer)
            {
                if (splitContainer.Panel1.Controls.Count > 0)
                {
                    return splitContainer.Panel1.Controls[0] as Panel;
                }
            }

            foreach (Control child in control.Controls)
            {
                var result = FindErdEditor(child);
                if (result != null) return result;
            }
            return null;
        }

        private ErdGraph13LB ConvertToErdGraph(MermaidGraphBase mermaidGraph)
        {
            var erdGraph = new ErdGraph13LB();

            foreach (var entity in mermaidGraph.Entities)
            {
                var erdEntity = new ErdEntity13LB
                {
                    Name = entity.Name,
                    BackgroundColor = Color.FromArgb(240, 240, 240),
                    Height = 30 + (entity.Fields.Count * 25)
                };

                foreach (var field in entity.Fields)
                {
                    erdEntity.Fields.Add(new ErdField13LB
                    {
                        Name = field.Name,
                        Type = field.Type,
                        Size = field.Size,
                        IsPrimaryKey = field.IsPrimaryKey,
                        IsForeignKey = field.IsForeignKey,
                        Constraints = field.Constraints,
                        DisplayOrder = erdEntity.Fields.Count
                    });
                }

                erdGraph.Entities.Add(erdEntity);
                erdGraph.EntityPositions[entity.Name] = CalculateEntityPosition(erdGraph.EntityPositions.Count);
            }

            foreach (var relation in mermaidGraph.Relations)
            {
                erdGraph.Relations.Add(new ErdRelation13LB
                {
                    From = relation.From,
                    To = relation.To,
                    Type = relation.Type,
                    Label = relation.Label,
                    LineColor = Color.FromArgb(100, 100, 100)
                });
            }

            return erdGraph;
        }

        private Point CalculateEntityPosition(int index)
        {
            int x = 50 + (index * 250) % 1000;
            int y = 50 + ((index * 250) / 1000) * 200;
            return new Point(x, y);
        }

        public async void HandleERD()
        {
            try
            {
                _logger.LogMethodEntry(nameof(HandleERD), nameof(ViewMenuHandler27IM));

                if (_erdDiagramPanel == null || _erdViewer == null)
                {
                    MessageBox.Show("ERD 다이어그램 패널을 찾을 수 없습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var editor = FindErdEditor(_mainForm);
                if (editor == null || editor.Controls.Count == 0)
                {
                    MessageBox.Show("ERD 에디터가 비어있습니다. 먼저 ERD 파일을 여세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string erdContent = string.Empty;
                if (editor.Controls[0] is FastColoredTextBox textBox)
                {
                    erdContent = textBox.Text;
                }

                if (string.IsNullOrWhiteSpace(erdContent))
                {
                    MessageBox.Show("ERD 내용이 비어있습니다. 먼저 ERD 파일을 여세요.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (erdContent != _lastValidContent || !_isErdContentValid)
                {
                    var mermaidJs = MermaidJs14LB.Instance;
                    var validationResult = await mermaidJs.ValidateMermaidAsync(erdContent);

                    if (!validationResult.Success)
                    {
                        MessageBox.Show($"ERD 구문 오류: {validationResult.ErrorMessage}", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _isErdContentValid = false;
                        _erdViewer.Visible = false;
                        _initialMessageLabel.Visible = true;
                        _initialMessageLabel.BringToFront();
                        return;
                    }

                    _isErdContentValid = true;
                    _lastValidContent = erdContent;
                }

                if (_isErdContentValid)
                {
                    _erdDiagramPanel.SuspendLayout();
                    _initialMessageLabel.Visible = false;
                    _erdViewer.Visible = true;
                    _erdViewer.BringToFront();

                    var mermaidGraph = Parse(erdContent);
                    var erdGraph = ConvertToErdGraph(mermaidGraph);
                    _erdHandler.RenderDiagram(erdGraph);

                    _erdDiagramPanel.ResumeLayout(true);
                    _logger.LogInformation("ERD viewer displayed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HandleERD: {ex.Message}", ex);
                MessageBox.Show("ERD 다이어그램 표시 중 오류가 발생했습니다.", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void HandleView()
        {
            try
            {
                _logger.LogMethodEntry(nameof(HandleView), nameof(ViewMenuHandler27IM));
                _logger.LogInformation("View menu handler called");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HandleView: {ex.Message}", ex);
            }
        }

        public void HandleDatabase()
        {
            try
            {
                _logger.LogMethodEntry(nameof(HandleDatabase), nameof(ViewMenuHandler27IM));
                _logger.LogInformation("Database menu handler called");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HandleDatabase: {ex.Message}", ex);
            }
        }

        public void HandleSync()
        {
            try
            {
                _logger.LogMethodEntry(nameof(HandleSync), nameof(ViewMenuHandler27IM));
                _logger.LogInformation("Sync menu handler called");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in HandleSync: {ex.Message}", ex);
            }
        }
    }
}