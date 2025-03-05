using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase17LB
{
    public abstract class BaseCanvas17LB : Control
    {
        protected readonly Logging14LB _logger;
        protected float _zoom = 1.0f;
        protected Point _dragStartPoint;
        protected bool _isDragging;
        protected BaseTableNode _selectedNode;
        protected Dictionary<string, BaseTableNode> _nodes;

        public class BaseTableNode
        {
            public string Name { get; set; }
            public Rectangle Bounds { get; set; }
            public List<BaseColumnInfo> Columns { get; set; }
            public string DatabaseName { get; set; }
            public bool IsSelected { get; set; }
            public Color GroupColor { get; set; }
        }

        public class BaseColumnInfo
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public bool IsPrimaryKey { get; set; }
            public bool IsForeignKey { get; set; }
        }

        public BaseCanvas17LB()
        {
            _logger = Logging14LB.Instance;
            _nodes = new Dictionary<string, BaseTableNode>();

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            MouseDown += BaseCanvas_MouseDown;
            MouseMove += BaseCanvas_MouseMove;
            MouseUp += BaseCanvas_MouseUp;
            MouseWheel += BaseCanvas_MouseWheel;
        }

        protected virtual void BaseCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragStartPoint = new Point((int)(e.X / _zoom), (int)(e.Y / _zoom));
                _selectedNode = FindNodeAt(_dragStartPoint);
                _isDragging = _selectedNode != null;

                if (_selectedNode != null)
                {
                    OnNodeSelected(_selectedNode);
                }
            }
        }

        protected virtual void BaseCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _selectedNode != null)
            {
                Point currentPoint = new Point((int)(e.X / _zoom), (int)(e.Y / _zoom));
                int dx = currentPoint.X - _dragStartPoint.X;
                int dy = currentPoint.Y - _dragStartPoint.Y;

                _selectedNode.Bounds = new Rectangle(
                    _selectedNode.Bounds.X + dx,
                    _selectedNode.Bounds.Y + dy,
                    _selectedNode.Bounds.Width,
                    _selectedNode.Bounds.Height);

                _dragStartPoint = currentPoint;
                OnNodeMoved(_selectedNode);
                Invalidate();
            }
        }

        protected virtual void BaseCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        protected virtual void BaseCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                float oldZoom = _zoom;
                if (e.Delta > 0)
                    _zoom = Math.Min(_zoom * 1.1f, 3.0f);
                else
                    _zoom = Math.Max(_zoom / 1.1f, 0.3f);

                if (_zoom != oldZoom)
                {
                    Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.ScaleTransform(_zoom, _zoom);

            DrawNodes(e.Graphics);
        }

        protected virtual void DrawNodes(Graphics g)
        {
            foreach (var node in _nodes.Values)
            {
                DrawTableBase(g, node);
            }
        }

        protected virtual void DrawTableBase(Graphics g, BaseTableNode node)
        {
            var bounds = node.Bounds;
            var headerHeight = 30;

            using (var bodyBrush = new SolidBrush(Color.White))
            using (var headerBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
            using (var borderPen = new Pen(node.IsSelected ? Color.Blue : Color.FromArgb(100, 100, 100),
                                         node.IsSelected ? 2 : 1))
            {
                // Draw body
                g.FillRectangle(bodyBrush, bounds);

                // Draw header
                var headerRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, headerHeight);
                g.FillRectangle(headerBrush, headerRect);

                // Draw table name
                using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
                {
                    g.DrawString(node.Name, font, Brushes.Black,
                        bounds.X + 5, bounds.Y + 5);
                }

                // Draw border
                g.DrawRectangle(borderPen, bounds);

                if (node.Columns != null)
                {
                    // Draw columns
                    float y = bounds.Y + headerHeight + 5;
                    using (var font = new Font("Segoe UI", 9))
                    {
                        foreach (var column in node.Columns)
                        {
                            string columnText = $"{column.Name}: {column.Type}";
                            if (column.IsPrimaryKey) columnText += " 🔑";
                            if (column.IsForeignKey) columnText += " 🔗";

                            g.DrawString(columnText, font, Brushes.Black,
                                bounds.X + 5, y);
                            y += 20;
                        }
                    }
                }
            }
        }

        protected virtual BaseTableNode FindNodeAt(Point location)
        {
            foreach (var node in _nodes.Values)
            {
                if (node.Bounds.Contains(location))
                {
                    return node;
                }
            }
            return null;
        }

        protected virtual void OnNodeSelected(BaseTableNode node)
        {
            foreach (var tableNode in _nodes.Values)
            {
                tableNode.IsSelected = (tableNode == node);
            }
            Invalidate();
        }

        protected virtual void OnNodeMoved(BaseTableNode node)
        {
            Invalidate();
        }

        protected void AddNode(BaseTableNode node)
        {
            if (!_nodes.ContainsKey(node.Name))
            {
                _nodes.Add(node.Name, node);
                Invalidate();
            }
        }

        protected BaseTableNode GetLastNode()
        {
            if (_nodes.Count == 0)
                return null;

            using (var enumerator = _nodes.Values.GetEnumerator())
            {
                var last = default(BaseTableNode);
                while (enumerator.MoveNext())
                {
                    last = enumerator.Current;
                }
                return last;
            }
        }

        public void Clear()
        {
            _nodes.Clear();
            Invalidate();
        }
    }
}