using System;
using System.Collections.Generic;

namespace FDCommon.CsBase3
{
    public class ErdBase3CB
    {
        public class TableInfo
        {
            public string TableName { get; set; }
            public string SchemaName { get; set; }
            public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();
            public List<string> PrimaryKeys { get; set; } = new List<string>();
            public List<ForeignKeyInfo> ForeignKeys { get; set; } = new List<ForeignKeyInfo>();
            public string GroupName { get; set; }
            public Point Location { get; set; }
        }

        public class ColumnInfo
        {
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public bool IsNullable { get; set; }
            public bool IsPrimaryKey { get; set; }
            public bool IsForeignKey { get; set; }
            public int MaxLength { get; set; }
        }

        public class ForeignKeyInfo
        {
            public string ConstraintName { get; set; }
            public string ReferencedTableName { get; set; }
            public string ReferencedColumnName { get; set; }
            public string ColumnName { get; set; }
        }

        public class GroupInfo
        {
            public string GroupName { get; set; }
            public List<string> TableNames { get; set; } = new List<string>();
            public Point Location { get; set; }
            public Size Size { get; set; }
        }

        public class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class Size
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }
    }
}