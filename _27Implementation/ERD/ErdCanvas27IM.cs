using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FDCommon.LibBase.LibBase14LB;
using FDCommon.LibBase.LibBase17LB;

namespace ERD2DB.Implementation.ERD
{
    public class ErdRelationEventArgs : EventArgs
    {
        public BaseCanvas17LB.BaseTableNode FromTable { get; }
        public BaseCanvas17LB.BaseTableNode ToTable { get; }

        public ErdRelationEventArgs(BaseCanvas17LB.BaseTableNode from, BaseCanvas17LB.BaseTableNode to)
        {
            FromTable = from;
            ToTable = to;
        }
    }

    public class ErdCanvas27IM : BaseCanvas17LB
    {
        private readonly List<BaseTableNode> selectedNodes = new List<BaseTableNode>();
        public event EventHandler<ErdRelationEventArgs> TableRelationCreated;

        public class TableNode : BaseTableNode { }

        public ErdCanvas27IM()
        {
        }

        protected override void OnNodeSelected(BaseTableNode node)
        {
            base.OnNodeSelected(node);
            if (node != null)
            {
                TableRelationCreated?.Invoke(this, new ErdRelationEventArgs(node, null));
            }
        }

        protected override void OnNodeMoved(BaseTableNode node)
        {
            base.OnNodeMoved(node);
        }

        public void AddTable(string name, List<BaseColumnInfo> columns, string database)
        {
            try
            {
                var table = new TableNode
                {
                    Name = name,
                    Columns = columns,
                    DatabaseName = database,
                    GroupColor = Color.FromArgb(240, 240, 240),
                    Bounds = GetNextTablePosition()
                };

                base.AddNode(table);
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

            var lastNode = base.GetLastNode();
            if (lastNode != null)
            {
                x = lastNode.Bounds.Right + 20;
                if (x + WIDTH > Width)
                {
                    x = 10;
                    y = lastNode.Bounds.Bottom + 20;
                }
            }

            return new Rectangle(x, y, WIDTH, HEIGHT);
        }
    }
}