using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Markdig;
using Models;

namespace BuildSite;

public class HtmlBuilder
{
    public string ContentPath { get; init; }
    public string DataPath { get; init; }
    public string WwwrootPath { get; init; }

    public HtmlBuilder()
    {
        ContentPath = Path.Combine(Environment.CurrentDirectory, "..", "..", BlogConst.ContentPath);
        DataPath = Path.Combine(Environment.CurrentDirectory, "..", "..", BlogConst.DataPath);
        WwwrootPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "src", "wwwroot");
    }
    public void BuildBlogs()
    {
        try
        {
            MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            List<string> files = Directory.EnumerateFiles(ContentPath, "*.md", SearchOption.AllDirectories)
                .ToList();

            files.ForEach(file =>
                {
                    string markdown = File.ReadAllText(file);
                    string html = Markdown.ToHtml(markdown, pipeline);
                    string relativePath = file.Replace(ContentPath, WwwrootPath).Replace(".md", ".html");

                    html = AddHtmlTags(html);
                    string? dir = Path.GetDirectoryName(relativePath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir!);
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

    public void BuildData()
    {
        var rootCatalog = new Catalog { Name = "Root" };
        TraverseDirectory(ContentPath, rootCatalog);
        string json = JsonSerializer.Serialize(rootCatalog.Children);

        string blogData = Path.Combine(DataPath, "blogs.json");
        File.WriteAllText(blogData, json, Encoding.UTF8);
    }


    public void TraverseDirectory(string directoryPath, Catalog parentCatalog)
    {
        foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath))
        {
            var catalog = new Catalog
            {
                Name = Path.GetFileName(subDirectoryPath),
                Parent = parentCatalog
            };
            parentCatalog.Children.Add(catalog);
            TraverseDirectory(subDirectoryPath, catalog);
        }

        foreach (string filePath in Directory.GetFiles(directoryPath, "*.md"))
        {
            var fileInfo = new FileInfo(filePath);
            var blog = new Blog
            {
                Title = GetTitleFromMarkdown(File.ReadAllText(filePath)),
                FileName = Path.GetFileName(filePath),
                PublishTime = DateTimeOffset.Now,
                CreatedTime = fileInfo.CreationTime,
                UpdatedTime = fileInfo.LastWriteTime,
                Catalog = parentCatalog
            };
            parentCatalog.Blogs.Add(blog);
        }
    }

    /// <summary>
    /// 获取标题
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private string GetTitleFromMarkdown(string content)
    {
        // 使用正则表达式匹配标题
        var regex = new Regex(@"^# (.*)$", RegexOptions.Multiline);
        var match = regex.Match(content);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return "";
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
