using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase17LB;

namespace ERD2DB.Implementation.ERD
{
    public class ErdEditorCanvas27IM : BaseCanvas17LB
    {
        private readonly Dictionary<string, TableNode> _tables;
        private readonly List<RelationLine> _relations;
        private TableNode _relationStartTable;
        private bool _isDrawingRelation;
        private Point _currentMousePoint;

        public event EventHandler<RelationEventArgs> RelationCreated;
        public event EventHandler<string> CodeGenerated;

        public class TableNode : BaseTableNode
        {
            public List<ConnectionPoint> ConnectionPoints { get; set; } = new List<ConnectionPoint>();
        }

        public class ConnectionPoint
        {
            public Point Location { get; set; }
            public Rectangle Bounds { get; set; }
            public RelationType Type { get; set; }
        }

        public class RelationLine
        {
            public TableNode FromTable { get; set; }
            public TableNode ToTable { get; set; }
            public RelationType Type { get; set; }
            public string Label { get; set; }
            public Point FromPoint { get; set; }
            public Point ToPoint { get; set; }
            public bool IsHighlighted { get; set; }
        }

        public enum RelationType
        {
            OneToOne,
            OneToMany,
            ManyToOne,
            ManyToMany
        }

        public ErdEditorCanvas27IM()
        {
            _tables = new Dictionary<string, TableNode>();
            _relations = new List<RelationLine>();

            this.KeyDown += ErdEditorCanvas_KeyDown;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw relations
            foreach (var relation in _relations)
            {
                DrawRelation(e.Graphics, relation);
            }

            // Draw temporary relation line while creating
            if (_isDrawingRelation && _relationStartTable != null)
            {
                using (var pen = new Pen(Color.Blue, 1))
                {
                    pen.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawLine(pen,
                        _relationStartTable.Bounds.Right,
                        _relationStartTable.Bounds.Top + _relationStartTable.Bounds.Height / 2,
                        _currentMousePoint.X,
                        _currentMousePoint.Y);
                }
            }

            // Draw tables
            foreach (var table in _tables.Values)
            {
                DrawTableBase(e.Graphics, table);
                DrawConnectionPoints(e.Graphics, table);
            }
        }

        private void DrawConnectionPoints(Graphics g, TableNode table)
        {
            foreach (var point in table.ConnectionPoints)
            {
                using (var brush = new SolidBrush(Color.LightGray))
                using (var pen = new Pen(Color.Gray, 1))
                {
                    g.FillEllipse(brush, point.Bounds);
                    g.DrawEllipse(pen, point.Bounds);
                }
            }
        }

        private void DrawRelation(Graphics g, RelationLine relation)
        {
            using (var pen = new Pen(relation.IsHighlighted ? Color.Blue : Color.Black, 1))
            {
                DrawRelationType(g, relation, pen);

                // Draw label if exists
                if (!string.IsNullOrEmpty(relation.Label))
                {
                    using (var font = new Font("Segoe UI", 8))
                    {
                        PointF labelPoint = new PointF(
                            (relation.FromPoint.X + relation.ToPoint.X) / 2,
                            (relation.FromPoint.Y + relation.ToPoint.Y) / 2 - 15);
                        g.DrawString(relation.Label, font, Brushes.Black, labelPoint);
                    }
                }
            }
        }

        private void DrawRelationType(Graphics g, RelationLine relation, Pen pen)
        {
            switch (relation.Type)
            {
                case RelationType.OneToOne:
                    DrawOneToOneRelation(g, relation, pen);
                    break;
                case RelationType.OneToMany:
                    DrawOneToManyRelation(g, relation, pen);
                    break;
                case RelationType.ManyToOne:
                    DrawManyToOneRelation(g, relation, pen);
                    break;
                case RelationType.ManyToMany:
                    DrawManyToManyRelation(g, relation, pen);
                    break;
            }
        }

        private void DrawOneToOneRelation(Graphics g, RelationLine relation, Pen pen)
        {
            g.DrawLine(pen, relation.FromPoint, relation.ToPoint);
            DrawEndPoint(g, relation.FromPoint, pen);
            DrawEndPoint(g, relation.ToPoint, pen);
        }

        private void DrawOneToManyRelation(Graphics g, RelationLine relation, Pen pen)
        {
            g.DrawLine(pen, relation.FromPoint, relation.ToPoint);
            DrawEndPoint(g, relation.FromPoint, pen);
            DrawCrowFoot(g, relation.ToPoint, pen);
        }

        private void DrawManyToOneRelation(Graphics g, RelationLine relation, Pen pen)
        {
            g.DrawLine(pen, relation.FromPoint, relation.ToPoint);
            DrawCrowFoot(g, relation.FromPoint, pen);
            DrawEndPoint(g, relation.ToPoint, pen);
        }

        private void DrawManyToManyRelation(Graphics g, RelationLine relation, Pen pen)
        {
            g.DrawLine(pen, relation.FromPoint, relation.ToPoint);
            DrawCrowFoot(g, relation.FromPoint, pen);
            DrawCrowFoot(g, relation.ToPoint, pen);
        }

        private void DrawEndPoint(Graphics g, Point point, Pen pen)
        {
            int size = 6;
            g.DrawLine(pen,
                point.X - size, point.Y - size,
                point.X - size, point.Y + size);
        }

        private void DrawCrowFoot(Graphics g, Point point, Pen pen)
        {
            int size = 6;
            g.DrawLine(pen,
                point.X, point.Y,
                point.X + size, point.Y - size);
            g.DrawLine(pen,
                point.X, point.Y,
                point.X + size, point.Y + size);
        }

        protected override void BaseCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var scaledPoint = new Point((int)(e.X / _zoom), (int)(e.Y / _zoom));
                var table = _tables.Values.FirstOrDefault(t => t.Bounds.Contains(scaledPoint));
                if (table != null)
                {
                    _relationStartTable = table;
                    _isDrawingRelation = true;
                }
            }
            else
            {
                base.BaseCanvas_MouseDown(sender, e);
            }
        }

        protected override void BaseCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            _currentMousePoint = new Point((int)(e.X / _zoom), (int)(e.Y / _zoom));
            base.BaseCanvas_MouseMove(sender, e);
            Invalidate();
        }

        protected override void BaseCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            base.BaseCanvas_MouseUp(sender, e);

            if (_isDrawingRelation && e.Button == MouseButtons.Right)
            {
                var endPoint = new Point((int)(e.X / _zoom), (int)(e.Y / _zoom));
                var targetTable = _tables.Values.FirstOrDefault(t => t.Bounds.Contains(endPoint));

                if (targetTable != null && targetTable != _relationStartTable)
                {
                    CreateRelation(_relationStartTable, targetTable);
                }

                _isDrawingRelation = false;
                _relationStartTable = null;
                Invalidate();
            }
        }

        private void CreateRelation(TableNode fromTable, TableNode toTable)
        {
            var relation = new RelationLine
            {
                FromTable = fromTable,
                ToTable = toTable,
                Type = RelationType.OneToMany,
                FromPoint = new Point(
                    fromTable.Bounds.Right,
                    fromTable.Bounds.Top + fromTable.Bounds.Height / 2),
                ToPoint = new Point(
                    toTable.Bounds.Left,
                    toTable.Bounds.Top + toTable.Bounds.Height / 2)
            };

            _relations.Add(relation);
            RelationCreated?.Invoke(this, new RelationEventArgs(fromTable, toTable));
            GenerateCode();
        }

        private void GenerateCode()
        {
            try
            {
                var code = new System.Text.StringBuilder();
                code.AppendLine("erDiagram");

                foreach (var table in _tables.Values)
                {
                    code.AppendLine($"    {table.Name} {{");
                    foreach (var column in table.Columns)
                    {
                        string constraints = "";
                        if (column.IsPrimaryKey) constraints += " PK";
                        if (column.IsForeignKey) constraints += " FK";
                        code.AppendLine($"        {column.Type} {column.Name}{constraints}");
                    }
                    code.AppendLine("    }");
                }

                foreach (var relation in _relations)
                {
                    string relationType = relation.Type switch
                    {
                        RelationType.OneToOne => "||--||",
                        RelationType.OneToMany => "||--o{",
                        RelationType.ManyToOne => "}o--||",
                        RelationType.ManyToMany => "}o--o{",
                        _ => "||--o{"
                    };

                    code.AppendLine($"    {relation.FromTable.Name} {relationType} {relation.ToTable.Name}");
                }

                CodeGenerated?.Invoke(this, code.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to generate code", ex);
            }
        }

        private void ErdEditorCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && _selectedNode != null)
            {
                var selectedTable = _selectedNode as TableNode;
                _relations.RemoveAll(r =>
                    r.FromTable == selectedTable || r.ToTable == selectedTable);
                _tables.Remove(selectedTable.Name);
                _selectedNode = null;
                GenerateCode();
                Invalidate();
            }
        }

        protected override BaseTableNode FindNodeAt(Point location)
        {
            return _tables.Values.FirstOrDefault(t => t.Bounds.Contains(location));
        }

        protected override void OnNodeSelected(BaseTableNode node)
        {
            foreach (var table in _tables.Values)
            {
                table.IsSelected = (table == node);
            }
            Invalidate();
        }

        protected override void OnNodeMoved(BaseTableNode node)
        {
            var movedTable = node as TableNode;
            if (movedTable != null)
            {
                // Update relation points
                foreach (var relation in _relations)
                {
                    if (relation.FromTable == movedTable)
                    {
                        relation.FromPoint = new Point(
                            movedTable.Bounds.Right,
                            movedTable.Bounds.Top + movedTable.Bounds.Height / 2);
                    }
                    if (relation.ToTable == movedTable)
                    {
                        relation.ToPoint = new Point(
                            movedTable.Bounds.Left,
                            movedTable.Bounds.Top + movedTable.Bounds.Height / 2);
                    }
                }

                GenerateCode();
            }
        }

        public void AddTable(string name, List<BaseColumnInfo> columns, string database)
        {
            try
            {
                if (!_tables.ContainsKey(name))
                {
                    var table = new TableNode
                    {
                        Name = name,
                        Columns = columns,
                        DatabaseName = database,
                        GroupColor = Color.FromArgb(240, 240, 240),
                        Bounds = GetNextTablePosition()
                    };

                    UpdateConnectionPoints(table);
                    _tables[name] = table;
                    GenerateCode();
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add table: {name}", ex);
            }
        }

        private Rectangle GetNextTablePosition()
        {
            int x = 10, y = 10;
            const int WIDTH = 200;
            const int HEIGHT = 200;

            if (_tables.Count > 0)
            {
                var lastTable = _tables.Values.Last();
                x = lastTable.Bounds.Right + 20;
                if (x + WIDTH > this.Width)
                {
                    x = 10;
                    y = lastTable.Bounds.Bottom + 20;
                }
            }

            return new Rectangle(x, y, WIDTH, HEIGHT);
        }

        private void UpdateConnectionPoints(TableNode table)
        {
            const int size = 6;
            table.ConnectionPoints = new List<ConnectionPoint>
            {
                new ConnectionPoint
                {
                    Location = new Point(table.Bounds.Left, table.Bounds.Top + table.Bounds.Height / 2),
                    Bounds = new Rectangle(
                        table.Bounds.Left - size / 2,
                        table.Bounds.Top + table.Bounds.Height / 2 - size / 2,
                        size, size)
                },
                new ConnectionPoint
                {
                    Location = new Point(table.Bounds.Right, table.Bounds.Top + table.Bounds.Height / 2),
                    Bounds = new Rectangle(
                        table.Bounds.Right - size / 2,
                        table.Bounds.Top + table.Bounds.Height / 2 - size / 2,
                        size, size)
                }
            };
        }

        public void Clear()
        {
            _tables.Clear();
            _relations.Clear();
            GenerateCode();
            Invalidate();
        }
    }

    public class RelationEventArgs : EventArgs
    {
        public BaseCanvas17LB.BaseTableNode FromTable { get; private set; }
        public BaseCanvas17LB.BaseTableNode ToTable { get; private set; }

        public RelationEventArgs(BaseCanvas17LB.BaseTableNode from, BaseCanvas17LB.BaseTableNode to)
        {
            FromTable = from;
            ToTable = to;
        }
    }
}