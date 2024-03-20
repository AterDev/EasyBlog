using System.Text;
using ColorCode;
using Markdig;

namespace BuildSite;

public class HtmlBuilder
{
    public void BuildBlogs()
    {
        var contentPath = Path.Combine(Environment.CurrentDirectory, "..", "Content");
        var wwwrootPath = Path.Combine(Environment.CurrentDirectory, "..", "src", "wwwroot");
        try
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();


            var files = Directory.EnumerateFiles(contentPath, "*.md", SearchOption.AllDirectories)
                .ToList();

            files.ForEach(file =>
                {
                    var markdown = File.ReadAllText(file);
                    var html = Markdig.Markdown.ToHtml(markdown, pipeline);
                    var relativePath = file.Replace(contentPath, wwwrootPath).Replace(".md", ".html");

                    html = AddHtmlTags(html);
                    var dir = Path.GetDirectoryName(relativePath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.WriteAllText(relativePath, html, Encoding.UTF8);
                });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw;
        }
    }

    private void Test()
    {
        var csharpstring = """
            module.exports = {
              content: ['./**/*.{razor,html}'],
              theme: {
                extend: {},
              },
              plugins: [],
            }

            """;
        var formatter = new HtmlClassFormatter();
        var html = formatter.GetHtmlString(csharpstring, Languages.Typescript);
        var css = formatter.GetCSSString();

        Console.WriteLine(html);
        Console.WriteLine(css);

    }

    private string AddHtmlTags(string content, string title = "")
    {
        var res = $"""
            <!DOCTYPE html>
            <html>
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width, initial-scale=1.0">
              <link rel="stylesheet" href="css/app.css">
              <link rel="stylesheet" href="css/markdown.css">
              <title>{title}</title>
            </head>
            <body>
            {content}
            </body>
            </html>
            """;
        return res;
    }
}
