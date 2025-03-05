using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FDCommon.CsBase.CsBase4CB
{
    public class ErdParser4CB
    {
        public ErdData4CB Parse(string content)
        {
            var data = new ErdData4CB();
            if (string.IsNullOrEmpty(content)) return data;

            // 엔티티 파싱
            var entityPattern = @"(\w+)\s*{([^}]*)}";
            var entityMatches = Regex.Matches(content, entityPattern);
            foreach (Match match in entityMatches)
            {
                var entity = ParseEntity(match);
                data.Entities.Add(entity);
            }

            // 관계 파싱
            var relationPattern = @"(\w+)\s*((\|\|--|}o--|\}\|--|\|\|\.\.|\}o\.\.|\}\|\.\.)(o\{|\|\{|\|\|))\s*(\w+)";
            var relationMatches = Regex.Matches(content, relationPattern);
            foreach (Match match in relationMatches)
            {
                var relation = ParseRelation(match);
                data.Relations.Add(relation);
            }

            return data;
        }

        private EntityData4CB ParseEntity(Match match)
        {
            var entity = new EntityData4CB
            {
                Name = match.Groups[1].Value,
                Fields = new List<FieldData4CB>()
            };

            var fieldsText = match.Groups[2].Value;
            var lines = fieldsText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                var field = ParseField(trimmedLine);
                if (field != null)
                {
                    entity.Fields.Add(field);
                }
            }

            return entity;
        }

        private FieldData4CB ParseField(string line)
        {
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return null;

            return new FieldData4CB
            {
                Name = parts[0],
                Type = parts[1],
                IsPrimaryKey = line.Contains("PK"),
                IsForeignKey = line.Contains("FK")
            };
        }

        private RelationData4CB ParseRelation(Match match)
        {
            return new RelationData4CB
            {
                FromEntity = match.Groups[1].Value,
                ToEntity = match.Groups[5].Value,
                RelationType = match.Groups[2].Value
            };
        }
    }

    public class ErdData4CB
    {
        public List<EntityData4CB> Entities { get; set; } = new List<EntityData4CB>();
        public List<RelationData4CB> Relations { get; set; } = new List<RelationData4CB>();
    }

    public class EntityData4CB
    {
        public string Name { get; set; }
        public List<FieldData4CB> Fields { get; set; }
    }

    public class FieldData4CB
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
    }

    public class RelationData4CB
    {
        public string FromEntity { get; set; }
        public string ToEntity { get; set; }
        public string RelationType { get; set; }
    }
}