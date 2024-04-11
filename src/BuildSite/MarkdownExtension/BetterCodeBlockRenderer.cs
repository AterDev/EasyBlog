using System.Text;
using ColorCode;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace BuildSite.MarkdownExtension;
internal class BetterCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
{
    private readonly CodeBlockRenderer _underlyingRenderer;
    public BetterCodeBlockRenderer(CodeBlockRenderer underlyingRenderer)
    {
        _underlyingRenderer = underlyingRenderer;
    }

    protected override void Write(HtmlRenderer renderer, CodeBlock obj)
    {
        if (obj is not FencedCodeBlock fencedCodeBlock || obj.Parser is not FencedCodeBlockParser parser || obj is null)
        {
            _underlyingRenderer.Write(renderer, obj);
            return;
        }
        var languageString = fencedCodeBlock.Info?.Replace(parser.InfoPrefix!, string.Empty);
        var language = Languages.FindById(string.IsNullOrWhiteSpace(languageString) ? "md" : languageString);
        if (language == null)
        {
            _underlyingRenderer.Write(renderer, obj);
            return;
        }

        var code = GetCode(obj)?.Trim();

        if (code != null)
        {
            var formatter = new HtmlClassFormatter();
            var html = formatter.GetHtmlString(code, language);
            renderer.WriteLine(html);
            return;
        }
        _underlyingRenderer.Write(renderer, obj);
        return;
    }


    private static string? GetCode(LeafBlock obj)
    {
        var str = new StringBuilder();
        if (obj.Lines.Count > 0)
        {
            foreach (var line in obj.Lines.Lines)
            {
                if (!string.IsNullOrWhiteSpace(line.Slice.ToString().Trim()))
                    str.AppendLine(line.Slice.ToString());
            }
            return str.ToString();
        }
        return null;
    }
}
