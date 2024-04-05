using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace BuildSite.MarkdownExtension;
internal class CodeBlockExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        if (renderer is not TextRendererBase<HtmlRenderer> htmlRenderer)
        {
            return;
        }

        var codeBlock = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
        if (codeBlock != null)
        {
            htmlRenderer.ObjectRenderers.Remove(codeBlock);
            htmlRenderer.ObjectRenderers.AddIfNotAlready(new BetterCodeBlockRenderer(codeBlock));

        }
    }
}
