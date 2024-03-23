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
        if (obj is not FencedCodeBlock fencedCodeBlock || obj.Parser is not FencedCodeBlockParser parser)
        {
            _underlyingRenderer.Write(renderer, obj);
            return;
        }

        var attributes = obj.TryGetAttributes() ?? new HtmlAttributes();

        var languageString = fencedCodeBlock.Info?.Replace(parser.InfoPrefix!, string.Empty);
        var language = Languages.FindById(languageString);
        if (language == null)
        {
            _underlyingRenderer.Write(renderer, obj);
            return;
        }

        var code = GetCode(obj);

        var formatter = new HtmlClassFormatter();
        var html = formatter.GetHtmlString(code, language);
        renderer.WriteLine(html);
    }

    private static string GetCode(LeafBlock obj)
    {
        var str = new StringBuilder();
        foreach (var line in obj.Lines.Lines)
        {
            if (!string.IsNullOrWhiteSpace(line.Slice.ToString().Trim()))
                str.AppendLine(line.Slice.ToString());
        }
        return str.ToString();
    }
}
