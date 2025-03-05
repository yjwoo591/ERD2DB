using CsvHelper.Configuration.Attributes;

namespace ERD2DB.Menu
{
    public class MenuData34ME
    {
        [Name("MenuID")]
        public string MenuID { get; set; }

        [Name("ParentID")]
        public string ParentID { get; set; }

        [Name("Text")]
        public string Text { get; set; }

        [Name("Shortcut")]
        public string Shortcut { get; set; }

        [Name("Handler_Class")]
        public string Handler_Class { get; set; }

        [Name("Handler_Method")]
        public string Handler_Method { get; set; }

        [Name("Enabled")]
        public bool Enabled { get; set; }

        [Name("Order")]
        public int Order { get; set; }
    }
}