using System;
using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Media = System.Windows.Media;

namespace ERD2DB.Implementation.ERD
{
    public class ErdSyntaxHighlight21I
    {
        private readonly IHighlightingDefinition syntaxHighlighting;

        public ErdSyntaxHighlight21I()
        {
            syntaxHighlighting = LoadHighlightingDefinition();
        }

        public IHighlightingDefinition GetSyntaxHighlighting()
        {
            return syntaxHighlighting;
        }

        private IHighlightingDefinition LoadHighlightingDefinition()
        {
            using (var stream = new MemoryStream(GetMermaidSyntaxDefinition()))
            using (var reader = new XmlTextReader(stream))
            {
                return HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
        }

        private byte[] GetMermaidSyntaxDefinition()
        {
            string xshd = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""Mermaid-ERD"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""Keyword"" foreground=""Blue"" fontWeight=""bold""/>
    <Color name=""EntityName"" foreground=""Navy"" fontWeight=""bold""/>
    <Color name=""DataType"" foreground=""Purple""/>
    <Color name=""Relationship"" foreground=""Green""/>
    <Color name=""Constraint"" foreground=""Red"" fontWeight=""bold""/>
    <Color name=""Comment"" foreground=""Green"" fontStyle=""italic""/>
    <Color name=""String"" foreground=""Brown""/>
    <Color name=""Number"" foreground=""DarkCyan""/>
    
    <RuleSet>
        <Span color=""Comment"" begin=""%"" />
        
        <Span color=""String"">
            <Begin>""</Begin>
            <End>""</End>
            <RuleSet>
                <Span begin=""""\"""" />
            </RuleSet>
        </Span>

        <Keywords color=""Keyword"">
            <Word>erDiagram</Word>
            <Word>graph</Word>
            <Word>subgraph</Word>
        </Keywords>

        <Keywords color=""DataType"">
            <Word>int</Word>
            <Word>string</Word>
            <Word>DateTime</Word>
            <Word>bool</Word>
            <Word>decimal</Word>
            <Word>float</Word>
            <Word>double</Word>
            <Word>char</Word>
            <Word>byte</Word>
            <Word>short</Word>
            <Word>long</Word>
        </Keywords>

        <Keywords color=""Constraint"">
            <Word>PK</Word>
            <Word>FK</Word>
            <Word>NOT NULL</Word>
            <Word>UNIQUE</Word>
            <Word>DEFAULT</Word>
            <Word>CHECK</Word>
            <Word>INDEX</Word>
        </Keywords>

        <Rule color=""Number"">
            \b[0-9]+\b
        </Rule>

        <Rule color=""EntityName"">
            \b[A-Za-z_][A-Za-z0-9_]*(?=\s*{)
        </Rule>

        <Rule color=""Relationship"">
            (\|\|--|}o--|\}\|--|\|\|\.\.|\}o\.\.|\}\|\.\.)(o\{|\|\{|\|\|)
        </Rule>
    </RuleSet>
</SyntaxDefinition>";

            return System.Text.Encoding.UTF8.GetBytes(xshd);
        }

        public static class EditorStyles
        {
            public static readonly Media.Color BackgroundColor = Media.Colors.White;
            public static readonly Media.Color ForegroundColor = Media.Colors.Black;
            public static readonly Media.Color LineNumbersForeground = Media.Colors.Gray;
            public static readonly Media.Color SelectionColor = Media.Color.FromArgb(50, 0, 120, 215);
            public static readonly Media.FontFamily EditorFont = new Media.FontFamily("Cascadia Code");
            public static readonly double FontSize = 12.0;
        }
    }
}