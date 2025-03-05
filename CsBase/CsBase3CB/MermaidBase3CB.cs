using System;
using System.Collections.Generic;

namespace ERD2DB.CsBase.CsBase3CB
{
    public interface IMermaidParser3CB
    {
        MermaidGraphBase3CB Parse(string mermaidCode);
    }

    public interface IMermaidRenderer3CB
    {
        Task<string> RenderDiagramAsync(MermaidGraphBase3CB graph);
    }

    public class MermaidGraphBase3CB
    {
        public List<EntityBase3CB> Entities { get; set; } = new List<EntityBase3CB>();
        public List<RelationBase3CB> Relations { get; set; } = new List<RelationBase3CB>();
    }

    public class EntityBase3CB
    {
        public string Name { get; set; }
        public List<FieldBase3CB> Fields { get; set; } = new List<FieldBase3CB>();
    }

    public class FieldBase3CB
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public List<string> Constraints { get; set; } = new List<string>();
    }

    public class RelationBase3CB
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
    }

    public class MermaidValidationResult3CB
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public static MermaidValidationResult3CB CreateSuccess()
        {
            return new MermaidValidationResult3CB { Success = true };
        }

        public static MermaidValidationResult3CB CreateError(string message)
        {
            return new MermaidValidationResult3CB
            {
                Success = false,
                ErrorMessage = message
            };
        }
    }
}