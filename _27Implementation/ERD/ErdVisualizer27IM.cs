using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;

namespace ERD2DB.Implementation.ERD
{
    public class ErdVisualizer27IM : Control
    {
        private readonly Dictionary<string, TableVisual> _tables;
        private readonly List<RelationVisual> _relations;
        private readonly HashSet<string> _highlightedTables;
        private readonly Logging14LB _logger;
        private float _zoom = 1.0f;
        private bool _isDragging;
        private TableVisual _selectedTable;
        private Point _dragStart;

        public class TableVisual
        {
            public string Name { get; set; }
            public Rectangle Bounds { get; set; }
            public List<ColumnVisual> Columns { get; set; }
            public Color BaseColor { get; set; }
            public bool IsHighlighted { get; set; }
            public string DatabaseName { get; set; }
        }

        public class ColumnVisual
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public bool IsPrimaryKey { get; set; }
            public bool IsForeignKey { get; set; }
            public string ReferencedTable { get; set; }
            public bool IsHighlighted { get; set; }
        }

        public class RelationVisual
        {
            public string FromTable { get; set; }
            public string ToTable { get; set; }
            public string Type { get; set; }
            public bool IsHighlighted { get; set; }
        }

        public ErdVisualizer27IM()
        {
            _logger = Logging14LB.Instance;
            _tables = new Dictionary<string, TableVisual>();
            _relations = new List<RelationVisual>();
            _highlightedTables = new HashSet<string>();

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            this.MouseDown += ErdVisualizer_MouseDown;
            this.MouseMove += ErdVisualizer_MouseMove;
            this.MouseUp += ErdVisualizer_MouseUp;
            this.MouseWheel += ErdVisualizer_MouseWheel;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.ScaleTransform(_zoom, _zoom);

            // Draw relations first (background)
            foreach (var relation in _relations)
            {
                DrawRelation(e.Graphics, relation);
            }

            // Draw tables
            foreach (var table in _tables.Values)
            {
                DrawTable(e.Graphics, table);
            }
        }

        private void DrawTable(Graphics g, TableVisual table)
        {
            var bounds = table.Bounds;
            var headerHeight = 30;

            using (var headerBrush = new SolidBrush(table.IsHighlighted ?
                Color.FromArgb(220, 230, 255) : table.BaseColor))
            using (var bodyBrush = new SolidBrush(table.IsHighlighted ?
                Color.FromArgb(240, 245, 255) : Color.White))
            using (var borderPen = new Pen(table.IsHighlighted ?
                Color.FromArgb(100, 140, 250) : Color.FromArgb(180, 180, 180),
                table.IsHighlighted ? 2 : 1))
            {
                // Draw the shadow
                if (table.IsHighlighted)
                {
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                    {
                        g.FillRectangle(shadowBrush,
                            bounds.X + 3, bounds.Y + 3, bounds.Width, bounds.Height);
                    }
                }

                // Draw table body
                g.FillRectangle(bodyBrush, bounds);

                // Draw header
                var headerRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, headerHeight);
                g.FillRectangle(headerBrush, headerRect);

                // Draw border
                g.DrawRectangle(borderPen, bounds);

                // Draw table name
                using (var headerFont = new Font("Segoe UI", 10, FontStyle.Bold))
                {
                    g.DrawString(table.Name, headerFont, Brushes.Black,
                        bounds.X + 5, bounds.Y + 5);
                }

                // Draw columns
                float y = bounds.Y + headerHeight + 5;
                using (var font = new Font("Segoe UI", 9))
                {
                    foreach (var column in table.Columns)
                    {
                        var columnBrush = column.IsHighlighted ?
                            Brushes.Blue : Brushes.Black;

                        string columnText = $"{column.Name}: {column.Type}";
                        if (column.IsPrimaryKey) columnText += " 🔑";
                        if (column.IsForeignKey) columnText += " 🔗";

                        g.DrawString(columnText, font, columnBrush,
                            bounds.X + 5, y);
                        y += 20;

                        // Draw line under each column
                        g.DrawLine(borderPen,
                            bounds.X, y - 2,
                            bounds.X + bounds.Width, y - 2);
                    }
                }
            }
        }

        private void DrawRelation(Graphics g, RelationVisual relation)
        {
            if (!_tables.TryGetValue(relation.FromTable, out var fromTable) ||
                !_tables.TryGetValue(relation.ToTable, out var toTable))
                return;

            using (var pen = new Pen(relation.IsHighlighted ?
                Color.FromArgb(100, 140, 250) : Color.FromArgb(100, 100, 100),
                relation.IsHighlighted ? 2 : 1))
            {
                // Calculate connection points
                Point fromPoint = new Point(
                    fromTable.Bounds.Right,
                    fromTable.Bounds.Y + fromTable.Bounds.Height / 2);
                Point toPoint = new Point(
                    toTable.Bounds.Left,
                    toTable.Bounds.Y + toTable.Bounds.Height / 2);

                // Create curved line
                using (GraphicsPath path = new GraphicsPath())
                {
                    float curveOffset = Math.Min(100, Math.Abs(toPoint.X - fromPoint.X) / 3);

                    path.AddBezier(
                        fromPoint,
                        new Point(fromPoint.X + (int)curveOffset, fromPoint.Y),
                        new Point(toPoint.X - (int)curveOffset, toPoint.Y),
                        toPoint);

                    if (relation.IsHighlighted)
                    {
                        // Draw highlight glow
                        using (var glowPen = new Pen(Color.FromArgb(50, 100, 140, 250), 6))
                        {
                            g.DrawPath(glowPen, path);
                        }
                    }

                    g.DrawPath(pen, path);
                }
            }
        }

        private void ErdVisualizer_MouseDown(object sender, MouseEventArgs e)
        {
            var scaledPoint = new Point(
                (int)Math.Round(e.X / _zoom),
                (int)Math.Round(e.Y / _zoom));

            _selectedTable = FindTableAt(scaledPoint);

            if (_selectedTable != null)
            {
                _isDragging = true;
                _dragStart = scaledPoint;
                HighlightRelatedTables(_selectedTable.Name);
            }
            else
            {
                HighlightRelatedTables(null, false);
            }
        }

        private void ErdVisualizer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _selectedTable != null)
            {
                var scaledPoint = new Point(
                    (int)Math.Round(e.X / _zoom),
                    (int)Math.Round(e.Y / _zoom));

                int dx = scaledPoint.X - _dragStart.X;
                int dy = scaledPoint.Y - _dragStart.Y;

                _selectedTable.Bounds = new Rectangle(
                    _selectedTable.Bounds.X + dx,
                    _selectedTable.Bounds.Y + dy,
                    _selectedTable.Bounds.Width,
                    _selectedTable.Bounds.Height);

                _dragStart = scaledPoint;
                Invalidate();
            }
        }

        private void ErdVisualizer_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private void ErdVisualizer_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                float oldZoom = _zoom;
                if (e.Delta > 0)
                    _zoom = Math.Min(_zoom * 1.1f, 3.0f);
                else
                    _zoom = Math.Max(_zoom / 1.1f, 0.3f);

                if (_zoom != oldZoom)
                    Invalidate();
            }
        }

        private TableVisual FindTableAt(Point location)
        {
            return _tables.Values.FirstOrDefault(t => t.Bounds.Contains(location));
        }

        private void HighlightRelatedTables(string tableName, bool highlight = true)
        {
            if (!_tables.ContainsKey(tableName))
                return;

            _highlightedTables.Clear();
            if (highlight)
            {
                // Add the main table
                _highlightedTables.Add(tableName);
                _tables[tableName].IsHighlighted = true;

                // Find all related tables through relations
                var relatedTables = _relations
                    .Where(r => r.FromTable == tableName || r.ToTable == tableName)
                    .Select(r => r.FromTable == tableName ? r.ToTable : r.FromTable);

                foreach (var related in relatedTables)
                {
                    _highlightedTables.Add(related);
                    _tables[related].IsHighlighted = true;
                }

                // Highlight relations
                foreach (var relation in _relations)
                {
                    relation.IsHighlighted = _highlightedTables.Contains(relation.FromTable) &&
                                           _highlightedTables.Contains(relation.ToTable);
                }
            }
            else
            {
                // Clear all highlights
                foreach (var table in _tables.Values)
                {
                    table.IsHighlighted = false;
                }
                foreach (var relation in _relations)
                {
                    relation.IsHighlighted = false;
                }
            }

            Invalidate();
        }

        public void AddTable(string name, List<ColumnVisual> columns, string database)
        {
            try
            {
                if (!_tables.ContainsKey(name))
                {
                    var table = new TableVisual
                    {
                        Name = name,
                        Columns = columns,
                        DatabaseName = database,
                        BaseColor = Color.FromArgb(240, 240, 240),
                        Bounds = GetNextTablePosition()
                    };

                    _tables[name] = table;
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add table: {name}", ex);
            }
        }

        public void AddRelation(string fromTable, string toTable, string type)
        {
            if (!_tables.ContainsKey(fromTable) || !_tables.ContainsKey(toTable))
                return;

            _relations.Add(new RelationVisual
            {
                FromTable = fromTable,
                ToTable = toTable,
                Type = type
            });

            Invalidate();
        }

        private Rectangle GetNextTablePosition()
        {
            int x = 10, y = 10;
            const int WIDTH = 200;
            const int HEIGHT = 200;

            if (_tables.Count > 0)
            {
                var lastTable = _tables.Values.Last();
                x = lastTable.Bounds.Right + 50;
                if (x + WIDTH > this.Width)
                {
                    x = 10;
                    y = _tables.Values.Max(t => t.Bounds.Bottom) + 50;
                }
            }

            return new Rectangle(x, y, WIDTH, HEIGHT);
        }

        public void Clear()
        {
            _tables.Clear();
            _relations.Clear();
            _highlightedTables.Clear();
            Invalidate();
        }
    }
}