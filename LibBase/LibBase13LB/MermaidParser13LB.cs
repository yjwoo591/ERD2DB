using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using FDCommon.CsBase.CsBase3CB;
using ERD2DB.CsBase.CsBase3CB;

namespace FDCommon.LibBase.LibBase13LB
{
    public class MermaidParser13LB : IMermaidParser3CB
    {
        protected readonly ILogger3CB _logger;
        protected static readonly Regex EntityPattern = new(@"(\w+)\s*{([^}]*)}",
            RegexOptions.Compiled | RegexOptions.Singleline);
        protected static readonly Regex RelationPattern = new(
            @"(\w+)\s*((\|\|--|}o--|\}\|--|\|\|\.\.|\}o\.\.|\}\|\.\.)(o\{|\|\{|\|\|))\s*(\w+)(?:\s*:\s*""([^""]*)"")?\s*",
            RegexOptions.Compiled);
        protected static readonly Regex FieldPattern = new Regex(
            @"^\s*(?<type>\w+)(?:\((?<size>\d+(?:,\d+)*)\))?\s+(?<name>\w+)(?:\s+(?<pk>PK))?(?:\s+(?<fk>FK))?(?:\s+(?<constraints>(?:NOT NULL|UNIQUE|DEFAULT|CHECK|INDEX)(?:\s+(?:NOT NULL|UNIQUE|DEFAULT|CHECK|INDEX))*))*\s*$",
            RegexOptions.Compiled);

        public MermaidParser13LB()
        {
            var loggerProvider = new DefaultLoggerProvider3CB();
            _logger = loggerProvider.GetLogger();
        }

        public MermaidGraphBase3CB Parse(string mermaidCode)
        {
            var graph = new MermaidGraphBase3CB();

            try
            {
                var lines = mermaidCode.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => !l.StartsWith("%%") && !string.IsNullOrWhiteSpace(l))
                    .ToList();

                lines = lines.Where(l => l != "erDiagram").ToList();

                EntityBase3CB currentEntity = null;

                foreach (var line in lines)
                {
                    if (line.Contains("{"))
                    {
                        var entityName = line.Split('{')[0].Trim();
                        currentEntity = new EntityBase3CB { Name = entityName };
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

        protected virtual void ParseField(string line, EntityBase3CB entity)
        {
            var match = FieldPattern.Match(line);
            if (match.Success)
            {
                var field = new FieldBase3CB
                {
                    Type = match.Groups["type"].Value,
                    Size = match.Groups["size"].Success ? match.Groups["size"].Value : null,
                    Name = match.Groups["name"].Value,
                    IsPrimaryKey = match.Groups["pk"].Success,
                    IsForeignKey = match.Groups["fk"].Success,
                    Constraints = new List<string>()
                };

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

        protected virtual void ParseRelation(string line, MermaidGraphBase3CB graph)
        {
            var match = RelationPattern.Match(line);
            if (match.Success)
            {
                var relation = new RelationBase3CB
                {
                    From = match.Groups[1].Value,
                    To = match.Groups[5].Value,
                    Type = match.Groups[2].Value,
                    Label = match.Groups[6].Success ? match.Groups[6].Value : null
                };

                graph.Relations.Add(relation);
            }
        }
    }
}