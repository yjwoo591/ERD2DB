using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase17LB;

namespace ERD2DB.Implementation.Database
{
    public class DatabaseCanvas27IM : BaseCanvas17LB
    {
        private readonly Dictionary<string, DatabaseGroup> _databases;
        private readonly Dictionary<string, TableNode> _tables;

        public event EventHandler<TableDragEventArgs> TableDragStarted;
        public event EventHandler<TableDragEventArgs> TableDragEnded;

        public class DatabaseGroup
        {
            public string Name { get; set; }
            public Rectangle Bounds { get; set; }
            public Color GroupColor { get; set; }
            public List<string> Tables { get; set; } = new List<string>();
        }

        public class TableNode : BaseTableNode { }

        public DatabaseCanvas27IM()
        {
            _databases = new Dictionary<string, DatabaseGroup>();
            _tables = new Dictionary<string, TableNode>();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw database groups first
            foreach (var db in _databases.Values)
            {
                DrawDatabaseGroup(e.Graphics, db);
            }

            // Draw tables
            foreach (var table in _tables.Values)
            {
                DrawTableBase(e.Graphics, table);
            }
        }

        private void DrawDatabaseGroup(Graphics g, DatabaseGroup db)
        {
            using (var brush = new SolidBrush(Color.FromArgb(30, db.GroupColor)))
            using (var pen = new Pen(Color.FromArgb(100, db.GroupColor), 2))
            using (var font = new Font("Segoe UI", 12, FontStyle.Bold))
            {
                g.FillRectangle(brush, db.Bounds);
                g.DrawRectangle(pen, db.Bounds);
                g.DrawString(db.Name, font, new SolidBrush(db.GroupColor),
                    db.Bounds.X + 10, db.Bounds.Y + 10);
            }
        }

        protected override BaseTableNode FindNodeAt(Point location)
        {
            foreach (var table in _tables.Values)
            {
                if (table.Bounds.Contains(location))
                {
                    return table;
                }
            }
            return null;
        }

        protected override void OnNodeSelected(BaseTableNode node)
        {
            foreach (var table in _tables.Values)
            {
                table.IsSelected = (table == node);
            }
            TableDragStarted?.Invoke(this, new TableDragEventArgs(node));
            Invalidate();
        }

        protected override void OnNodeMoved(BaseTableNode node)
        {
            // Nothing special needed for database canvas node movement
        }

        protected override void BaseCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            base.BaseCanvas_MouseUp(sender, e);
            if (_selectedNode != null)
            {
                TableDragEnded?.Invoke(this, new TableDragEventArgs(_selectedNode));
            }
        }

        public void AddDatabase(string name)
        {
            if (!_databases.ContainsKey(name))
            {
                int y = 10;
                if (_databases.Count > 0)
                {
                    y = _databases.Values.Max(db => db.Bounds.Bottom) + 20;
                }

                _databases[name] = new DatabaseGroup
                {
                    Name = name,
                    GroupColor = GetDatabaseColor(_databases.Count),
                    Bounds = new Rectangle(10, y, Width - 20, 200)
                };
                Invalidate();
            }
        }

        private Color GetDatabaseColor(int index)
        {
            Color[] colors = {
                Color.FromArgb(200, 220, 240), // Light Blue
                Color.FromArgb(200, 240, 200), // Light Green
                Color.FromArgb(240, 220, 200), // Light Orange
                Color.FromArgb(220, 200, 240)  // Light Purple
            };
            return colors[index % colors.Length];
        }

        public void AddTable(string database, string tableName, List<BaseColumnInfo> columns)
        {
            try
            {
                if (!_databases.ContainsKey(database))
                {
                    _logger.LogError($"Database not found: {database}");
                    return;
                }

                if (!_tables.ContainsKey(tableName))
                {
                    var dbGroup = _databases[database];
                    var table = new TableNode
                    {
                        Name = tableName,
                        DatabaseName = database,
                        Columns = columns,
                        GroupColor = dbGroup.GroupColor,
                        Bounds = GetTablePosition(dbGroup)
                    };

                    _tables[tableName] = table;
                    dbGroup.Tables.Add(tableName);
                    AdjustDatabaseSize(dbGroup);
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add table: {tableName}", ex);
            }
        }

        private Rectangle GetTablePosition(DatabaseGroup dbGroup)
        {
            const int TABLE_WIDTH = 150;
            const int TABLE_HEIGHT = 100;
            int x = dbGroup.Bounds.X + 10;
            int y = dbGroup.Bounds.Y + 40;

            if (dbGroup.Tables.Count > 0)
            {
                var lastTable = _tables[dbGroup.Tables.Last()];
                x = lastTable.Bounds.Right + 20;
                if (x + TABLE_WIDTH > dbGroup.Bounds.Right - 10)
                {
                    x = dbGroup.Bounds.X + 10;
                    y = lastTable.Bounds.Bottom + 20;
                }
            }

            return new Rectangle(x, y, TABLE_WIDTH, TABLE_HEIGHT);
        }

        private void AdjustDatabaseSize(DatabaseGroup dbGroup)
        {
            if (dbGroup.Tables.Count > 0)
            {
                var tables = dbGroup.Tables.Select(t => _tables[t]);
                int maxY = tables.Max(t => t.Bounds.Bottom);
                if (maxY + 20 > dbGroup.Bounds.Bottom)
                {
                    dbGroup.Bounds = new Rectangle(
                        dbGroup.Bounds.X,
                        dbGroup.Bounds.Y,
                        dbGroup.Bounds.Width,
                        maxY - dbGroup.Bounds.Y + 20
                    );

                    // Adjust positions of databases below
                    AdjustDatabasePositions(dbGroup.Name);
                }
            }
        }

        private void AdjustDatabasePositions(string startFromDatabase)
        {
            bool adjust = false;
            int newY = 10;

            foreach (var db in _databases.Values)
            {
                if (adjust || db.Name == startFromDatabase)
                {
                    if (adjust)
                    {
                        int yDiff = newY - db.Bounds.Y;
                        db.Bounds = new Rectangle(
                            db.Bounds.X,
                            newY,
                            db.Bounds.Width,
                            db.Bounds.Height
                        );

                        // Adjust tables in this database
                        foreach (var tableName in db.Tables)
                        {
                            var table = _tables[tableName];
                            table.Bounds = new Rectangle(
                                table.Bounds.X,
                                table.Bounds.Y + yDiff,
                                table.Bounds.Width,
                                table.Bounds.Height
                            );
                        }
                    }
                    newY = db.Bounds.Bottom + 20;
                    adjust = true;
                }
            }
        }

        public void Clear()
        {
            _databases.Clear();
            _tables.Clear();
            Invalidate();
        }
    }

    public class TableDragEventArgs : EventArgs
    {
        public BaseCanvas17LB.BaseTableNode Table { get; private set; }

        public TableDragEventArgs(BaseCanvas17LB.BaseTableNode table)
        {
            Table = table;
        }
    }
}