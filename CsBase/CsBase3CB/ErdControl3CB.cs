using System;
using System.Collections.Generic;
using ERD2DB.CsBase.CsBase1CB;

namespace ERD2DB.CsBase.CsBase3CB
{
    public class ErdControl3CB
    {
        public class Table3CB
        {
            public string Name { get; set; }
            public GeometryBase1CB.Point1CB Location { get; set; }
            public GeometryBase1CB.Size1CB Size { get; set; }
            public List<Field3CB> Fields { get; set; } = new List<Field3CB>();
        }

        public class Field3CB
        {
            public string Name { get; set; }
            public string DataType { get; set; }
            public bool IsPrimaryKey { get; set; }
            public bool IsForeignKey { get; set; }
            public string Description { get; set; }
        }

        public class Relationship3CB
        {
            public string FromTable { get; set; }
            public string ToTable { get; set; }
            public RelationType3CB Type { get; set; }
        }

        public enum RelationType3CB
        {
            OneToOne,
            OneToMany,
            ManyToMany
        }
    }
}