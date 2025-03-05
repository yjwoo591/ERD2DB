using System;
using System.Threading.Tasks;
using System.Linq;
using FDCommon.CsBase.CsBase3CB;
using FDCommon.LibBase.LibBase13LB;
using ERD2DB.CsBase.CsBase3CB;

namespace FDCommon.LibBase.LibBase14LB
{
    public class MermaidJs14LB : IMermaidRenderer3CB
    {
        private static MermaidJs14LB _instance;
        private readonly ILogger3CB _logger;
        private readonly MermaidParser13LB _parser;

        public static MermaidJs14LB Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MermaidJs14LB();
                }
                return _instance;
            }
        }

        private MermaidJs14LB()
        {
            var loggerProvider = new DefaultLoggerProvider3CB();
            _logger = loggerProvider.GetLogger();
            _parser = new MermaidParser13LB();
        }

        public async Task<(bool Success, string ErrorMessage)> ValidateMermaidAsync(string content)
        {
            try
            {
                _logger.LogMethodEntry(nameof(ValidateMermaidAsync), nameof(MermaidJs14LB));

                if (string.IsNullOrWhiteSpace(content))
                {
                    return (false, "Mermaid 내용이 비어있습니다.");
                }

                if (!IsMermaidContent(content))
                {
                    return (false, "유효한 Mermaid 다이어그램이 아닙니다.");
                }

                var (isValid, errorMessage) = await Task.Run(() => ValidateERDStructure(content));
                return (isValid, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Mermaid validation error: {ex.Message}", ex);
                return (false, ex.Message);
            }
            finally
            {
                _logger.LogMethodExit(nameof(ValidateMermaidAsync), nameof(MermaidJs14LB));
            }
        }

        private bool IsMermaidContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            var normalizedContent = new string(content.Where(c => !char.IsWhiteSpace(c)).ToArray());
            return normalizedContent.Contains("erDiagram");
        }

        private (bool isValid, string errorMessage) ValidateERDStructure(string content)
        {
            try
            {
                _parser.Parse(content);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"ERD 구조 검증 중 오류 발생: {ex.Message}");
            }
        }

        public MermaidGraphBase3CB Parse(string content)
        {
            try
            {
                return _parser.Parse(content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to parse Mermaid content: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<string> RenderDiagramAsync(MermaidGraphBase3CB graph)
        {
            try
            {
                return await Task.Run(() => GenerateSvgDiagram(graph));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to render diagram: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<string> RenderDiagramFromContentAsync(string content)
        {
            try
            {
                var validationResult = await ValidateMermaidAsync(content);
                if (!validationResult.Success)
                {
                    throw new Exception(validationResult.ErrorMessage);
                }

                var graph = _parser.Parse(content);
                return await RenderDiagramAsync(graph);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to render diagram from content: {ex.Message}", ex);
                throw;
            }
        }

        private string GenerateSvgDiagram(MermaidGraphBase3CB graph)
        {
            var svg = new System.Text.StringBuilder();
            svg.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"800\" height=\"600\">");

            int yOffset = 50;
            foreach (var entity in graph.Entities)
            {
                svg.AppendLine($"<g transform=\"translate(50,{yOffset})\">");
                svg.AppendLine($"<rect width=\"200\" height=\"30\" fill=\"#f0f0f0\" stroke=\"#000\"/>");
                svg.AppendLine($"<text x=\"10\" y=\"20\">{entity.Name}</text>");
                svg.AppendLine("</g>");
                yOffset += 50;
            }

            svg.AppendLine("</svg>");
            return svg.ToString();
        }
    }
}