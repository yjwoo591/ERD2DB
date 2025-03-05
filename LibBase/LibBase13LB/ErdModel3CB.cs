using System;
using System.Collections.Generic;
using System.Drawing;

namespace FDCommon.LibBase.LibBase13LB
{
    public interface IErdHandler13LB
    {
        void RenderDiagram(ErdGraph13LB graph);
        void UpdateZoom(bool zoomIn);
        void Clear();
    }

    public class ErdGraph13LB
    {
        public List<ErdEntity13LB> Entities { get; set; } = new List<ErdEntity13LB>();
        public List<ErdRelation13LB> Relations { get; set; } = new List<ErdRelation13LB>();
        public Dictionary<string, Point> EntityPositions { get; set; } = new Dictionary<string, Point>();
        public Dictionary<string, Color> EntityColors { get; set; } = new Dictionary<string, Color>();
        public Dictionary<string, string> DatabaseAssignments { get; set; } = new Dictionary<string, string>();
    }

    public class ErdEntity13LB
    {
        public string Name { get; set; }
        public List<ErdField13LB> Fields { get; set; } = new List<ErdField13LB>();
        public string DatabaseName { get; set; }
        public Color BackgroundColor { get; set; }
        public bool IsHighlighted { get; set; }
        public int Width { get; set; } = 200;
        public int Height { get; set; }
        public List<string> ReferencedBy { get; set; } = new List<string>();
        public List<string> References { get; set; } = new List<string>();
    }

    public class ErdField13LB
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public List<string> Constraints { get; set; } = new List<string>();
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsNullable { get; set; }
        public string DefaultValue { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ErdRelation13LB
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string FromField { get; set; }
        public string ToField { get; set; }
        public bool IsCascadeDelete { get; set; }
        public bool IsCascadeUpdate { get; set; }
        public Color LineColor { get; set; }
        public int LineThickness { get; set; } = 1;
        public List<Point> IntermediatePoints { get; set; } = new List<Point>();
    }
}