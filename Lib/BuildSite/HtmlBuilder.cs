using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using BuildSite.MarkdownExtension;
using Markdig;
using Models;

namespace BuildSite;

public partial class HtmlBuilder
{
    public string ContentPath { get; init; }
    public string DataPath { get; init; }
    public string WwwrootPath { get; init; }
    public string BaseUrl { get; set; } = "/";

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true
    };

    public HtmlBuilder()
    {
        ContentPath = Path.Combine(Environment.CurrentDirectory, BlogConst.ContentPath);
        WwwrootPath = Path.Combine(Environment.CurrentDirectory, "src", "wwwroot");
        DataPath = Path.Combine(WwwrootPath, BlogConst.DataPath);
    }
    public void BuildBlogs()
    {
        BaseUrl = GetBaseUrl();
        // 配置markdown管道
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseBetterCodeBlock()
            .Build();

        // 读取所有要处理的md文件
        List<string> files = Directory.EnumerateFiles(ContentPath, "*.md", SearchOption.AllDirectories)
            .ToList();
        try
        {
            files.ForEach(file =>
                {
                    string markdown = File.ReadAllText(file);
                    string html = Markdown.ToHtml(markdown, pipeline);
                    string relativePath = file.Replace(ContentPath, Path.Combine(WwwrootPath, "blogs")).Replace(".md", ".html");

                    html = AddHtmlTags(html);
                    string? dir = Path.GetDirectoryName(relativePath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir!);
                    }

                    File.WriteAllText(relativePath, html, Encoding.UTF8);
                });
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void BuildData()
    {
        var rootCatalog = new Catalog { Name = "Root" };
        TraverseDirectory(ContentPath, rootCatalog);
        string json = JsonSerializer.Serialize(rootCatalog, _jsonSerializerOptions);

        if (!Directory.Exists(DataPath))
        {
            Directory.CreateDirectory(DataPath);
        }
        string blogData = Path.Combine(DataPath, "blogs.json");
        File.WriteAllText(blogData, json, Encoding.UTF8);
    }

    private void TraverseDirectory(string directoryPath, Catalog parentCatalog)
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
            var fileName = Path.GetFileName(filePath);
            var blog = new Blog
            {
                Title = Path.GetFileNameWithoutExtension(filePath),
                FileName = fileName,
                Path = string.Empty,
                PublishTime = fileInfo.LastWriteTime,
                CreatedTime = fileInfo.CreationTime,
                UpdatedTime = fileInfo.LastWriteTime,
                Catalog = parentCatalog
            };
            blog.Path = GetFullPath(parentCatalog) + "/" + Uri.EscapeDataString(blog.FileName.Replace(".md", ".html"));

            parentCatalog.Blogs.Add(blog);
        }
    }

    private string GetFullPath(Catalog catalog)
    {
        var path = Uri.EscapeDataString(catalog.Name);
        if (catalog.Parent != null)
        {
            path = GetFullPath(catalog.Parent) + "/" + path;
        }
        return path.Replace("Root", "");
    }
    private string GetBaseUrl()
    {
        var indexPath = Path.Combine(WwwrootPath, "index.html");
        if (File.Exists(indexPath))
        {
            // get base tag content
            var content = File.ReadAllText(indexPath);
            var regex = new Regex(@"<base href=""(.*)"" />");
            var match = regex.Match(content);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

        }
        return "/";
    }

    /// <summary>
    /// 获取标题
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private string GetTitleFromMarkdown(string content)
    {
        // 使用正则表达式匹配标题
        var regex = TitleRegex();
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
              <link rel="stylesheet" href="{BaseUrl}css/app.css">
              <link rel="stylesheet" href="{BaseUrl}css/markdown.css">
              <title>{title}</title>
            </head>
            <body class="container mx-auto px-4 sm:px-6 lg:px-8 dark:bg-neutral-900 pb-4">
            {content}

            </body>
            </html>
            """;
        return res;
    }

    [GeneratedRegex(@"^# (.*)$", RegexOptions.Multiline)]
    private static partial Regex TitleRegex();
}
