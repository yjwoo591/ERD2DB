using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using FDCommon.LibBase.LibBase14LB;

namespace FDCommon.LibBase.LibBase19LB
{
    public class MermaidParser19LB
    {
        protected readonly Logging14LB _logger;
        protected static readonly Regex EntityPattern = new(@"(\w+)\s*{([^}]*)}",
            RegexOptions.Compiled | RegexOptions.Singleline);
        protected static readonly Regex RelationPattern = new(
            @"(\w+)\s*((\|\|--|}o--|\}\|--|\|\|\.\.|\}o\.\.|\}\|\.\.)(o\{|\|\{|\|\|))\s*(\w+)(?:\s*:\s*""([^""]*)"")?\s*",
            RegexOptions.Compiled);
        protected static readonly Regex FieldPattern = new Regex(
            @"^\s*(?<type>\w+)(?:\((?<size>\d+(?:,\d+)*)\))?\s+(?<name>\w+)(?:\s+(?<pk>PK))?(?:\s+(?<fk>FK))?(?:\s+(?<constraints>(?:NOT NULL|UNIQUE|DEFAULT|CHECK|INDEX)(?:\s+(?:NOT NULL|UNIQUE|DEFAULT|CHECK|INDEX))*))*\s*$",
            RegexOptions.Compiled);

        public class MermaidGraphBase
        {
            public List<EntityBase> Entities { get; set; } = new List<EntityBase>();
            public List<RelationBase> Relations { get; set; } = new List<RelationBase>();
        }

        public class EntityBase
        {
            public string Name { get; set; }
            public List<FieldBase> Fields { get; set; } = new List<FieldBase>();
        }

        public class FieldBase
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Size { get; set; }
            public bool IsPrimaryKey { get; set; }
            public bool IsForeignKey { get; set; }
            public List<string> Constraints { get; set; } = new List<string>();
        }

        public class RelationBase
        {
            public string From { get; set; }
            public string To { get; set; }
            public string Type { get; set; }
            public string Label { get; set; }
        }

        public MermaidParser19LB()
        {
            _logger = Logging14LB.Instance;
        }

        protected virtual MermaidGraphBase CreateGraph()
        {
            return new MermaidGraphBase();
        }

        protected virtual EntityBase CreateEntity()
        {
            return new EntityBase();
        }

        protected virtual FieldBase CreateField()
        {
            return new FieldBase();
        }

        protected virtual RelationBase CreateRelation()
        {
            return new RelationBase();
        }

        public virtual MermaidGraphBase Parse(string mermaidCode)
        {
            var graph = CreateGraph();

            try
            {
                var lines = mermaidCode.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => !l.StartsWith("%%") && !string.IsNullOrWhiteSpace(l))
                    .ToList();

                lines = lines.Where(l => l != "erDiagram").ToList();

                EntityBase currentEntity = null;

                foreach (var line in lines)
                {
                    if (line.Contains("{"))
                    {
                        var entityName = line.Split('{')[0].Trim();
                        currentEntity = CreateEntity();
                        currentEntity.Name = entityName;
                        graph.Entities.Add(currentEntity);
                        continue;
                    }

                    if (line.Contains("}"))
                    {
                        currentEntity = null;
                        continue;
                    }

                    if (currentEntity != null)
                    {
                        ParseField(line, currentEntity);
                    }
                    else if (line.Contains("--"))
                    {
                        ParseRelation(line, graph);
                    }
                }

                return graph;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Mermaid parsing error: {ex.Message}", ex);
                throw new Exception($"Mermaid 파싱 오류: {ex.Message}");
            }
        }

        protected virtual void ParseField(string line, EntityBase entity)
        {
            var match = FieldPattern.Match(line);
            if (match.Success)
            {
                var field = CreateField();
                field.Type = match.Groups["type"].Value;
                field.Size = match.Groups["size"].Success ? match.Groups["size"].Value : null;
                field.Name = match.Groups["name"].Value;
                field.IsPrimaryKey = match.Groups["pk"].Success;
                field.IsForeignKey = match.Groups["fk"].Success;

                if (match.Groups["constraints"].Success)
                {
                    field.Constraints.AddRange(
                        match.Groups["constraints"].Value.Split(
                            new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    );
                }

                entity.Fields.Add(field);
            }
        }

        protected virtual void ParseRelation(string line, MermaidGraphBase graph)
        {
            var parts = line.Split(new[] { "--", ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var relation = CreateRelation();
                relation.From = parts[0].Trim();
                relation.To = parts[1].Trim();
                relation.Label = parts.Length > 2 ? parts[2].Trim().Trim('"') : null;
                graph.Relations.Add(relation);
            }
        }
    }
}