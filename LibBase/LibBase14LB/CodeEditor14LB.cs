using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;

namespace FDCommon.LibBase.LibBase14LB
{
    public class CodeEditor14LB : IDisposable
    {
        private FastColoredTextBox _textBox;
        private readonly Logging14LB _logger;
        private const float MIN_FONT_SIZE = 8.0f;
        private const float MAX_FONT_SIZE = 72.0f;
        private const float FONT_SIZE_STEP = 1.0f;
        private ToolStripStatusLabel _fontSizeLabel;

        private readonly TextStyle KeywordStyle;
        private readonly TextStyle EntityStyle;
        private readonly TextStyle PropertyStyle;
        private readonly TextStyle RelationStyle;
        private readonly TextStyle CommentStyle;
        private readonly TextStyle StringStyle;
        private readonly TextStyle TypeStyle;

        public string Text
        {
            get => _textBox?.Text ?? string.Empty;
            set
            {
                if (_textBox != null)
                {
                    _textBox.Text = value;
                    _textBox.ClearUndo();
                }
            }
        }

        public Control EditorControl => _textBox;

        public event EventHandler<TextChangedEventArgs> TextChanged
        {
            add { if (_textBox != null) _textBox.TextChanged += value; }
            remove { if (_textBox != null) _textBox.TextChanged -= value; }
        }

        public CodeEditor14LB(Logging14LB logger)
        {
            _logger = logger;
            _logger.LogMethodEntry(nameof(CodeEditor14LB), nameof(CodeEditor14LB));

            try
            {
                KeywordStyle = new TextStyle(Brushes.Blue, null, FontStyle.Bold);
                EntityStyle = new TextStyle(Brushes.DarkBlue, null, FontStyle.Bold);
                PropertyStyle = new TextStyle(Brushes.Purple, null, FontStyle.Regular);
                RelationStyle = new TextStyle(Brushes.Green, null, FontStyle.Regular);
                CommentStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
                StringStyle = new TextStyle(Brushes.Brown, null, FontStyle.Regular);
                TypeStyle = new TextStyle(Brushes.DarkCyan, null, FontStyle.Regular);

                InitializeFontSizeLabel();
                InitializeTextBox();

                _logger.LogInformation("CodeEditor initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize CodeEditor", ex);
                throw;
            }
        }

        private void InitializeFontSizeLabel()
        {
            try
            {
                _fontSizeLabel = new ToolStripStatusLabel
                {
                    AutoSize = true,
                    BorderSides = ToolStripStatusLabelBorderSides.All,
                    BorderStyle = Border3DStyle.SunkenOuter,
                    Text = "폰트 크기: 12.0"
                };
                _logger.LogInformation("Font size label initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize font size label", ex);
                throw;
            }
        }

        private void InitializeTextBox()
        {
            try
            {
                _textBox = new FastColoredTextBox
                {
                    Dock = DockStyle.Fill,
                    Language = Language.Custom,
                    Font = new Font("Consolas", 12),
                    ShowLineNumbers = true,
                    TabLength = 4,
                    WordWrap = true,
                    AutoIndent = true,
                    AutoIndentChars = true,
                    CaretColor = Color.Black,
                    LineNumberColor = Color.Gray,
                    IndentBackColor = Color.WhiteSmoke,
                    BackColor = Color.White,
                    ForeColor = Color.Black
                };

                _textBox.TextChanged += TextBox_TextChanged;
                _textBox.MouseWheel += TextBox_MouseWheel;

                UpdateFontSizeLabel();
                _logger.LogInformation("TextBox initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize TextBox", ex);
                throw;
            }
        }

        public string GetEditorText(Control editor)
        {
            try
            {
                _logger.LogMethodEntry(nameof(GetEditorText), nameof(CodeEditor14LB));

                if (editor == null)
                {
                    throw new ArgumentNullException(nameof(editor));
                }

                string text;
                if (editor is FastColoredTextBox textBox)
                {
                    text = textBox.Text;
                }
                else if (editor == _textBox)
                {
                    text = _textBox.Text;
                }
                else
                {
                    throw new ArgumentException("Invalid editor control type");
                }

                _logger.LogInformation($"Retrieved editor text, length: {text?.Length ?? 0}");
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get editor text: {ex.Message}", ex);
                throw;
            }
        }

        public void SetEditorText(Control editor, string text)
        {
            try
            {
                _logger.LogMethodEntry(nameof(SetEditorText), nameof(CodeEditor14LB));

                if (editor == null)
                {
                    throw new ArgumentNullException(nameof(editor));
                }

                if (editor is FastColoredTextBox textBox)
                {
                    textBox.Text = text;
                    textBox.ClearUndo();
                    _logger.LogInformation($"Editor text set successfully, length: {text?.Length ?? 0}");
                }
                else if (editor == _textBox)
                {
                    _textBox.Text = text;
                    _textBox.ClearUndo();
                    _logger.LogInformation($"CodeEditor text set successfully, length: {text?.Length ?? 0}");
                }
                else
                {
                    throw new ArgumentException("Invalid editor control type");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to set editor text: {ex.Message}", ex);
                throw;
            }
        }

        private void TextBox_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    float newSize = _textBox.Font.Size;
                    if (e.Delta > 0 && newSize < MAX_FONT_SIZE)
                    {
                        newSize += FONT_SIZE_STEP;
                    }
                    else if (e.Delta < 0 && newSize > MIN_FONT_SIZE)
                    {
                        newSize -= FONT_SIZE_STEP;
                    }

                    if (newSize != _textBox.Font.Size)
                    {
                        _textBox.Font = new Font(_textBox.Font.FontFamily, newSize, _textBox.Font.Style);
                        UpdateFontSizeLabel();
                        _logger.LogInformation($"Font size changed to: {newSize}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adjusting font size", ex);
            }
        }

        private void UpdateFontSizeLabel()
        {
            try
            {
                if (_fontSizeLabel != null && _textBox != null)
                {
                    _fontSizeLabel.Text = $"폰트 크기: {_textBox.Font.Size:0.#}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating font size label", ex);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (e?.ChangedRange == null) return;

                e.ChangedRange.ClearStyle(KeywordStyle, EntityStyle, PropertyStyle,
                    RelationStyle, CommentStyle, StringStyle, TypeStyle);

                e.ChangedRange.SetStyle(KeywordStyle, @"\b(erDiagram|graph|subgraph)\b");
                e.ChangedRange.SetStyle(EntityStyle, @"\b[A-Za-z_][A-Za-z0-9_]*(?=\s*{)");
                e.ChangedRange.SetStyle(TypeStyle,
                    @"\b(int|string|DateTime|bool|decimal|float|double|char|byte|short|long)\b");
                e.ChangedRange.SetStyle(RelationStyle,
                    @"(\|\|--|}o--|\}\|--|\|\|\.\.|\}o\.\.|\}\|\.\.)(o\{|\|\{|\|\|)");
                e.ChangedRange.SetStyle(CommentStyle, @"%.*$", RegexOptions.Multiline);
                e.ChangedRange.SetStyle(StringStyle, @""".*?""");
                e.ChangedRange.SetStyle(PropertyStyle, @"\b(PK|FK|NOT NULL|UNIQUE|DEFAULT|CHECK|INDEX)\b");

                _logger.LogInformation("Syntax highlighting applied successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error applying syntax highlighting", ex);
            }
        }

        public ToolStripStatusLabel GetFontSizeLabel()
        {
            return _fontSizeLabel;
        }

        public void Dispose()
        {
            try
            {
                _textBox?.Dispose();
                _fontSizeLabel?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error disposing CodeEditor", ex);
            }
        }
    }
}