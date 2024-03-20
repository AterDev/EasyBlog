using System.Text;
using ColorCode;
using Markdig;

namespace BuildSite;

public class HtmlBuilder
{
    public void BuildBlogs()
    {
        string contentPath = Path.Combine(Environment.CurrentDirectory, "..", "Content");
        string wwwrootPath = Path.Combine(Environment.CurrentDirectory, "..", "src", "wwwroot");
        try
        {
            MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();


            List<string> files = Directory.EnumerateFiles(contentPath, "*.md", SearchOption.AllDirectories)
                .ToList();

            files.ForEach(file =>
                {
                    string markdown = File.ReadAllText(file);
                    string html = Markdig.Markdown.ToHtml(markdown, pipeline);
                    string relativePath = file.Replace(contentPath, wwwrootPath).Replace(".md", ".html");

                    html = AddHtmlTags(html);
                    string? dir = Path.GetDirectoryName(relativePath);
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
        string csharpstring = """
            module.exports = {
              content: ['./**/*.{razor,html}'],
              theme: {
                extend: {},
              },
              plugins: [],
            }

            """;
        HtmlClassFormatter formatter = new HtmlClassFormatter();
        string html = formatter.GetHtmlString(csharpstring, Languages.Typescript);
        string css = formatter.GetCSSString();

        Console.WriteLine(html);
        Console.WriteLine(css);

    }

    private string AddHtmlTags(string content, string title = "")
    {
        string res = $"""
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
