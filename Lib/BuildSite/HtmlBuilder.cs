using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;

using BuildSite.MarkdownExtension;

using Humanizer;

using Markdig;

using Models;

namespace BuildSite;

public partial class HtmlBuilder
{
    public string ContentPath { get; init; }
    public string Output { get; init; }
    public string DataPath { get; init; }
    public string BaseUrl { get; set; } = "/";

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true
    };

    public HtmlBuilder(string input, string output)
    {
        Output = Path.Combine(output);
        ContentPath = input.EndsWith(Path.DirectorySeparatorChar) ? input[0..^1] : input;
        DataPath = Path.Combine(Output, BlogConst.DataPath);
    }

    public void BuildWebSite()
    {
        BuildData();
        BuildBlogs();
        BuildIndex();
    }

    /// <summary>
    /// blog html file
    /// </summary>
    public void BuildBlogs()
    {
        // 配置markdown管道
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseAutoIdentifiers(Markdig.Extensions.AutoIdentifiers.AutoIdentifierOptions.GitHub)
            .UseTaskLists()
            .UseAlertBlocks()
            .UseEmojiAndSmiley()
            .UseMathematics()
            .UseMediaLinks()
            .UseBetterCodeBlock()
            .Build();

        // 读取所有要处理的md文件
        List<string> files = Directory.EnumerateFiles(ContentPath, "*.md", SearchOption.AllDirectories)
            .ToList();
        try
        {
            foreach (var file in files)
            {
                string markdown = File.ReadAllText(file);
                string html = Markdig.Markdown.ToHtml(markdown, pipeline);
                string relativePath = file.Replace(ContentPath, Path.Combine(Output, "blogs")).Replace(".md", ".html");

                var title = GetTitleFromMarkdown(markdown);
                var toc = GetTOC(markdown) ?? "";
                html = AddHtmlTags(html, title, toc);
                string? dir = Path.GetDirectoryName(relativePath);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir!);
                }

                File.WriteAllText(relativePath, html, Encoding.UTF8);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// json 数据文件
    /// </summary>
    public void BuildData()
    {
        if (!Directory.Exists(DataPath))
        {
            Directory.CreateDirectory(DataPath);
        }

        // copy webinfo.json
        var webInfoPath = Path.Combine(Environment.CurrentDirectory, "webinfo.json");
        if (File.Exists(webInfoPath))
        {
            File.Copy(webInfoPath, Path.Combine(DataPath, "webinfo.json"), true);
        }

        // blogs
        var rootCatalog = new Catalog { Name = "Root" };
        TraverseDirectory(ContentPath, rootCatalog);
        string json = JsonSerializer.Serialize(rootCatalog, _jsonSerializerOptions);


        string blogData = Path.Combine(DataPath, "blogs.json");
        File.WriteAllText(blogData, json, Encoding.UTF8);
    }

    /// <summary>
    /// 构建 index.html
    /// </summary>
    public void BuildIndex()
    {
        var indexPath = Path.Combine(Output, "index.html");
        var indexHtml = TemplateHelper.GetTplContent("index.html");
        var webInfoPath = Path.Combine(DataPath, "webinfo.json");
        var content = File.ReadAllText(webInfoPath);
        var webInfo = JsonSerializer.Deserialize<WebInfo>(content);
        var blogData = Path.Combine(DataPath, "blogs.json");
        var blogContent = File.ReadAllText(blogData);
        var rootCatalog = JsonSerializer.Deserialize<Catalog>(blogContent);

        if (rootCatalog != null && webInfo != null)
        {
            var blogHtml = GenBlogListHtml(rootCatalog, webInfo);
            var siderBarHtml = GenSiderBar(rootCatalog);

            indexHtml = indexHtml.Replace("@{Name}", webInfo.Name)
                .Replace("@{BaseUrl}", BaseUrl)
                .Replace("@{Description}", webInfo.Description)
                .Replace("@{blogList}", blogHtml)
                .Replace("@{siderbar}", siderBarHtml);

            File.WriteAllText(indexPath, indexHtml, Encoding.UTF8);
        }
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
                PublishTime = GetUpdatedTime(filePath) ?? fileInfo.LastWriteTime,
                CreatedTime = GetCreatedTime(filePath) ?? fileInfo.CreationTime,
                UpdatedTime = GetUpdatedTime(filePath) ?? fileInfo.LastWriteTime,
                Catalog = parentCatalog
            };

            blog.Path = GetFullPath(parentCatalog) + "/" + Uri.EscapeDataString(blog.FileName.Replace(".md", ".html"));

            parentCatalog.Blogs.Add(blog);
        }
    }

    private static string GetFullPath(Catalog catalog)
    {
        var path = Uri.EscapeDataString(catalog.Name);
        if (catalog.Parent != null)
        {
            path = GetFullPath(catalog.Parent) + "/" + path;
        }
        return path.Replace("Root", "");
    }
    public void EnableBaseUrl()
    {
        var webInfoPath = Path.Combine(Environment.CurrentDirectory, "webinfo.json");
        if (File.Exists(webInfoPath))
        {
            var content = File.ReadAllText(webInfoPath);
            var webInfo = JsonSerializer.Deserialize<WebInfo>(content);
            BaseUrl = webInfo?.BaseHref ?? "/";
        }
    }

    /// <summary>
    /// 获取标题
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private static string GetTitleFromMarkdown(string content)
    {
        // 使用正则表达式匹配标题
        var regex = TitleRegex();
        var match = regex.Match(content);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }
        return "";
    }
    private string AddHtmlTags(string content, string title = "", string toc = "")
    {
        var tplContent = TemplateHelper.GetTplContent("blog.html");
        tplContent = tplContent.Replace("@{Title}", title)
            .Replace("@{BaseUrl}", BaseUrl)
            .Replace("@{toc}", toc)
            .Replace("@{content}", content);
        return tplContent;
    }

    [GeneratedRegex(@"^# (.*)$", RegexOptions.Multiline)]
    private static partial Regex TitleRegex();

    private static DateTimeOffset? GetCreatedTime(string path)
    {
        if (ProcessHelper.RunCommand("git", @$"log --diff-filter=A --format=%aI -- ""{path}""", out string output))
        {
            output = output.Split("\n").First();
            return ConvertToDateTimeOffset(output);
        }
        return null;
    }

    private static DateTimeOffset? GetUpdatedTime(string path)
    {
        if (ProcessHelper.RunCommand("git", @$"log -n 1 --format=%aI -- ""{path}""", out string output))
        {
            return ConvertToDateTimeOffset(output);
        }
        return null;
    }

    private static DateTimeOffset ConvertToDateTimeOffset(string output)
    {
        var dateString = output.Trim();

        string format = "yyyy-MM-ddTHH:mm:sszzz"; // 定义日期时间格式
        DateTimeOffset dateTimeOffset = DateTimeOffset.ParseExact(dateString, format, System.Globalization.CultureInfo.InvariantCulture);
        return dateTimeOffset;
    }

    /// <summary>
    /// blog ist html
    /// </summary>
    /// <returns></returns>
    private string GenBlogListHtml(Catalog rootCatalog, WebInfo webInfo)
    {
        var sb = new StringBuilder();
        if (rootCatalog == null) return string.Empty;
        var blogs = rootCatalog.GetAllBlogs().OrderByDescending(b => b.PublishTime).ToList() ?? [];

        foreach (var blog in blogs)
        {
            var html = $"""
                   <div class="w-100 rounded overflow-hidden shadow-lg dark:bg-neutral-800 my-2">
                       <div class="px-6 py-3">
                           <div class="font-bold text-xl mb-2">
                               <a href = "{BuildBlogPath(blog.Path)}" target="_blank" class="block text-lg py-2 text-neutral-600 hover:text-neutral-800 dark:text-neutral-300 dark:hover:text-neutral-100">📑 {blog.Title}</a>
                           </div>
                           <p class="text-neutral-700 text-base dark:text-neutral-300">
                               👨‍💻 {webInfo?.AuthorName}
                               &nbsp;&nbsp;
                               ⏱️ {blog.PublishTime.ToString("yyyy-MM-dd")}
                           </p>
                       </div>
                   </div>
                   """;
            sb.AppendLine(html);

        }
        return sb.ToString();
    }

    /// <summary>
    /// catalog and date
    /// </summary>
    /// <returns></returns>
    private string GenSiderBar(Catalog data)
    {
        var sb = new StringBuilder();
        var catalogs = data?.Children.ToList() ?? [];
        var allBlogs = data?.GetAllBlogs().OrderByDescending(b => b.PublishTime).ToList() ?? [];
        var dates = allBlogs!.Select(b => b.PublishTime)
            .OrderByDescending(b => b)
            .DistinctBy(b => b.Date)
            .ToList();

        sb.AppendLine("<div id=\"catalog-list\" class=\"rounded-lg shadow-md p-4 dark:bg-neutral-800\">");
        sb.AppendLine("<div class=\"text-xl font-semibold dark:text-neutral-300\">分类</div>");
        sb.AppendLine($"""
            <span data-catalog="all" class="filter-item text-lg block py-2 text-neutral-600 hover:text-neutral-800 dark:text-neutral-300 dark:hover:text-neutral-100">
                全部 [{allBlogs.Count}]
            </span>
            """);
        foreach (var catalog in catalogs)
        {
            var html = $"""
                <span data-catalog="{catalog.Name}" class="filter-item text-lg block py-2 text-neutral-600 hover:text-neutral-800 dark:text-neutral-300 dark:hover:text-neutral-100">
                    {catalog.Name} [{catalog.Blogs.Count}]
                </span>
                """;

            sb.AppendLine(html);
        }
        sb.AppendLine("</div>");

        sb.AppendLine("<div id=\"date-list\" class=\"rounded-lg shadow-md p-4 dark:bg-neutral-800 mt-2\">");
        sb.AppendLine("<div class=\"text-xl font-semibold dark:text-neutral-300\">存档</div>");
        sb.AppendLine($"""
            <span data-date="all" class="filter-item text-lg block py-2 text-neutral-600 hover:text-neutral-800 dark:text-neutral-300 dark:hover:text-neutral-100">
                全部 [{allBlogs.Count}]
            </span>
            """);
        foreach (var date in dates)
        {
            var count = allBlogs.Count(b => b.PublishTime.Date == date.Date);
            var html = $"""
                <span data-date="{date:yyyy-MM-dd}" class="filter-item text-lg block py-2 text-neutral-600 hover:text-neutral-800 dark:text-neutral-300 dark:hover:text-neutral-100">
                    {date:yyyy-MM-dd} [{count}]
                </span>
                """;
            sb.AppendLine(html);
        }
        sb.AppendLine("</div>");

        return sb.ToString();
    }
    private string? GetTOC(string markdown)
    {
        string heading2Pattern = @"^##\s+(.+)$";

        MatchCollection matches = Regex.Matches(markdown, heading2Pattern, RegexOptions.Multiline);

        if (matches.Count > 0)
        {
            var tocBuilder = new StringBuilder();
            tocBuilder.AppendLine(" <p class=\"text-lg\">导航</p>");
            tocBuilder.AppendLine(@"<ul class=""toc"">");

            foreach (Match match in matches)
            {
                string headingText = match.Groups[1].Value;
                string headingId = headingText.ToLower().Replace(" ", "-");
                tocBuilder.AppendLine($"  <li><a href='#{headingId}'>{headingText}</a></li>");
            }
            tocBuilder.AppendLine("</ul>");
            return tocBuilder.ToString();
        }
        return null;
    }

    private string FormatDatetime(DateTimeOffset dateTime)
    {
        TimeSpan timeDifference = DateTimeOffset.Now - dateTime;
        return timeDifference.Humanize();
    }
    private string BuildBlogPath(string path)
    {
        return BaseUrl + "blogs" + path;
    }



}
