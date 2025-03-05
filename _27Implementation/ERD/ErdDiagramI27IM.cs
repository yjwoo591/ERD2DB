using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using FDCommon.LibBase.LibBase13LB;
using FDCommon.LibBase.LibBase14LB;
using Krypton.Toolkit;

namespace ERD2DB.Implementation.ERD
{
    public class ErdDiagram21I : IErdHandler13LB
    {
        private readonly ErdPanel21I mainPanel;
        private readonly Logging14LB _logger;
        private readonly KryptonManager14LB _kryptonManager;
        private float zoomFactor = 1.0f;
        private ErdGraph13LB currentGraph;

        public ErdDiagram21I()
        {
            _logger = Logging14LB.Instance;
            _kryptonManager = KryptonManager14LB.Instance;
            mainPanel = new ErdPanel21I();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Paint += MainPanel_Paint;
        }

        public Control GetViewer()
        {
            return mainPanel;
        }

        public void RenderDiagram(ErdGraph13LB graph)
        {
            try
            {
                _logger.LogMethodEntry(nameof(RenderDiagram), nameof(ErdDiagram21I));
                currentGraph = graph;
                mainPanel.Invalidate();
                _logger.LogInformation("Diagram rendered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to render diagram: {ex.Message}", ex);
                throw;
            }
        }

        public void Clear()
        {
            currentGraph = null;
            mainPanel.Invalidate();
        }

        public void UpdateZoom(bool zoomIn)
        {
            float oldZoom = zoomFactor;
            if (zoomIn)
            {
                zoomFactor = Math.Min(zoomFactor * 1.2f, 3.0f);
            }
            else
            {
                zoomFactor = Math.Max(zoomFactor / 1.2f, 0.3f);
            }

            if (oldZoom != zoomFactor)
            {
                mainPanel.Invalidate();
            }
        }

        private void MainPanel_Paint(object sender, PaintEventArgs e)
        {
            if (currentGraph == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.ScaleTransform(zoomFactor, zoomFactor);

            // Draw relations first
            foreach (var relation in currentGraph.Relations)
            {
                DrawRelation(e.Graphics, relation);
            }

            // Draw entities
            foreach (var entity in currentGraph.Entities)
            {
                DrawEntity(e.Graphics, entity);
            }
        }

        private void DrawEntity(Graphics g, ErdEntity13LB entity)
        {
            if (!currentGraph.EntityPositions.TryGetValue(entity.Name, out Point location))
                return;

            var bounds = new Rectangle(location, new Size(entity.Width, entity.Height));
            var headerHeight = 30;

            using (var headerBrush = new SolidBrush(entity.BackgroundColor))
            using (var bodyBrush = new SolidBrush(Color.White))
            using (var borderPen = new Pen(entity.IsHighlighted ? Color.Blue : Color.Gray, 1))
            {
                // Draw background
                g.FillRectangle(bodyBrush, bounds);

                // Draw header
                g.FillRectangle(headerBrush, bounds.X, bounds.Y, bounds.Width, headerHeight);

                // Draw border
                g.DrawRectangle(borderPen, bounds);

                // Draw title
                using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
                {
                    g.DrawString(entity.Name, font, Brushes.Black,
                        bounds.X + 5, bounds.Y + 5);
                }

                // Draw fields
                float y = bounds.Y + headerHeight;
                using (var font = new Font("Segoe UI", 9))
                {
                    foreach (var field in entity.Fields)
                    {
                        string fieldText = $"{field.Name}: {field.Type}";
                        if (field.IsPrimaryKey) fieldText += " 🔑";
                        if (field.IsForeignKey) fieldText += " 🔗";

                        g.DrawString(fieldText, font, Brushes.Black,
                            bounds.X + 5, y + 5);
                        g.DrawLine(borderPen, bounds.X, y, bounds.X + bounds.Width, y);
                        y += 25;
                    }
                }
            }
        }

        private void DrawRelation(Graphics g, ErdRelation13LB relation)
        {
            if (!currentGraph.EntityPositions.TryGetValue(relation.From, out Point fromPos) ||
                !currentGraph.EntityPositions.TryGetValue(relation.To, out Point toPos))
                return;

            using (var pen = new Pen(relation.LineColor, relation.LineThickness))
            {
                pen.CustomEndCap = new AdjustableArrowCap(6, 6);

                if (relation.IntermediatePoints.Count > 0)
                {
                    var points = new List<Point> { fromPos };
                    points.AddRange(relation.IntermediatePoints);
                    points.Add(toPos);
                    g.DrawLines(pen, points.ToArray());
                }
                else
                {
                    g.DrawLine(pen, fromPos, toPos);
                }

                // Draw label if exists
                if (!string.IsNullOrEmpty(relation.Label))
                {
                    var midPoint = new PointF(
                        (fromPos.X + toPos.X) / 2,
                        (fromPos.Y + toPos.Y) / 2 - 15);

                    using (var font = new Font("Segoe UI", 8))
                    {
                        g.DrawString(relation.Label, font, Brushes.Black, midPoint);
                    }
                }
            }
        }

        public void Dispose()
        {
            mainPanel?.Dispose();
        }
    }
}