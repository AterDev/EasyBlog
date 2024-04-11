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
    public string Output { get; init; }
    public string DataPath { get; init; }
    public string BaseUrl { get; set; } = "/";

    public WebInfo WebInfo { get; init; }

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true
    };

    public HtmlBuilder(string input, string output)
    {
        Output = output;
        ContentPath = input.EndsWith(Path.DirectorySeparatorChar) ? input[0..^1] : input;
        DataPath = Path.Combine(Output, BlogConst.DataPath);

        var webInfoPath = Path.Combine(Environment.CurrentDirectory, "webinfo.json");
        if (File.Exists(webInfoPath))
        {
            var content = File.ReadAllText(webInfoPath);
            WebInfo = JsonSerializer.Deserialize<WebInfo>(content) ?? new WebInfo();
        }
        else
        {
            WebInfo = new WebInfo();
        }
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
            .UseAlertBlocks()
            .UseFigures()
            .UseCitations()
            .UseFigures()
            .UseEmphasisExtras()
            .UseMathematics()
            .UseMediaLinks()
            .UseListExtras()
            .UseTaskLists()
            .UseDiagrams()
            .UseAutoLinks()
            .UseAutoIdentifiers(Markdig.Extensions.AutoIdentifiers.AutoIdentifierOptions.GitHub)
            .UsePipeTables()
            .UseBetterCodeBlock()
            .Build();

        // 读取所有要处理的md文件
        List<string> files = Directory.EnumerateFiles(ContentPath, "*.md", SearchOption.AllDirectories)
            .ToList();
        // 复制其他非md文件
        List<string> otherFiles = Directory.EnumerateFiles(ContentPath, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".md"))
            .ToList();

        foreach (var file in files)
        {
            try
            {
                string markdown = File.ReadAllText(file);

                string html = Markdown.ToHtml(markdown, pipeline);
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
            catch (Exception e)
            {
                Console.WriteLine($"❌ parse markdown error: {file}" + e.Message + e.StackTrace);
            }

        }
        Console.WriteLine("✅ generate blog html!");

        foreach (var file in otherFiles)
        {
            string relativePath = file.Replace(ContentPath, Path.Combine(Output, "blogs"));
            string? dir = Path.GetDirectoryName(relativePath);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir!);
            }

            File.Copy(file, relativePath, true);
        }
        Console.WriteLine("✅ copy blog other files!");


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

            Console.WriteLine("✅ copy webinfo.json!");
        }

        // 获取git历史信息
        ProcessHelper.RunCommand("git", "fetch --unshallow", out string _);

        // create blogs.json
        var rootCatalog = new Catalog { Name = "Root" };
        TraverseDirectory(ContentPath, rootCatalog);
        string json = JsonSerializer.Serialize(rootCatalog, _jsonSerializerOptions);

        string blogData = Path.Combine(DataPath, "blogs.json");
        File.WriteAllText(blogData, json, Encoding.UTF8);
        Console.WriteLine("✅ update blogs.json!");

        // create sitemap.xml
        var blogs = rootCatalog.GetAllBlogs();
        BuildSitemap(blogs);
    }

    /// <summary>
    /// 构建 index.html
    /// </summary>
    public void BuildIndex()
    {
        var indexPath = Path.Combine(Output, "index.html");
        var indexHtml = TemplateHelper.GetTplContent("index.html");
        var blogData = Path.Combine(DataPath, "blogs.json");
        var blogContent = File.ReadAllText(blogData);
        var rootCatalog = JsonSerializer.Deserialize<Catalog>(blogContent);

        if (rootCatalog != null && WebInfo != null)
        {
            var blogHtml = GenBlogListHtml(rootCatalog, WebInfo);
            var siderBarHtml = GenSiderBar(rootCatalog);

            indexHtml = indexHtml.Replace("@{Name}", WebInfo.Name)
                .Replace("@{BaseUrl}", BaseUrl)
                .Replace("@{Description}", WebInfo.Description)
                .Replace("@{blogList}", blogHtml)
                .Replace("@{siderbar}", siderBarHtml);

            File.WriteAllText(indexPath, indexHtml, Encoding.UTF8);
            Console.WriteLine("✅ update index.html");
        }
    }

    private void TraverseDirectory(string directoryPath, Catalog parentCatalog)
    {
        foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath))
        {
            var existMd = Directory.GetFiles(subDirectoryPath, "*.md").Length > 0;
            if (!existMd) { continue; }
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
            var gitAddTime = GetCreatedTime(filePath);
            var gitUpdateTime = GetUpdatedTime(filePath);
            var blog = new Blog
            {
                Title = Path.GetFileNameWithoutExtension(filePath),
                FileName = fileName,
                Path = string.Empty,
                PublishTime = gitUpdateTime ?? gitAddTime ?? fileInfo.LastWriteTime,
                CreatedTime = gitAddTime ?? fileInfo.CreationTime,
                UpdatedTime = gitUpdateTime ?? fileInfo.LastWriteTime,
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
        BaseUrl = WebInfo?.BaseHref ?? "/";
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
        string extensionHead = "";
        if (content.Contains("<div class=\"mermaid\">"))
        {
            extensionHead += "<script src=\"https://cdn.jsdelivr.net/npm/mermaid@10.9.0/dist/mermaid.min.js\"></script>" + Environment.NewLine;
        }
        if (content.Contains("<div class=\"math\">"))
        {
            extensionHead += """
                <script src="https://polyfill.io/v3/polyfill.min.js?features=es6"></script>
                <script id="MathJax-script" async src="https://cdn.jsdelivr.net/npm/mathjax@3.0.1/es5/tex-mml-chtml.js"></script>
                
                """;
        }
        if (content.Contains("<div class=\"nomnoml\">"))
        {
            extensionHead += """
                <script src="//unpkg.com/graphre/dist/graphre.js"></script>
                <script src="//unpkg.com/nomnoml/dist/nomnoml.js"></script>
                """;
        }

        var tplContent = TemplateHelper.GetTplContent("blog.html");
        tplContent = tplContent.Replace("@{Title}", title)
            .Replace("@{BaseUrl}", BaseUrl)
            .Replace("@{ExtensionHead}", extensionHead)
            .Replace("@{toc}", toc)
            .Replace("@{content}", content);
        return tplContent;
    }


    /// <summary>
    /// 创建sitemap.xml
    /// </summary>
    private void BuildSitemap(List<Blog> blogs)
    {
        if (!string.IsNullOrWhiteSpace(WebInfo.Domain) && blogs.Count > 0)
        {
            var sitemaps = new List<Sitemap>();
            var domain = WebInfo.Domain.EndsWith('/') ? WebInfo.Domain[..^1] : WebInfo.Domain;
            foreach (var blog in blogs)
            {
                var sitemap = new Sitemap
                {
                    Loc = domain + BuildBlogPath(blog.Path),
                    Lastmod = blog.PublishTime.ToString("yyyy-MM-dd")
                };
                sitemaps.Add(sitemap);
            }

            var sitemapXml = Sitemap.GetSitemaps(sitemaps);
            var sitemapPath = Path.Combine(Output, "sitemap.xml");
            File.WriteAllText(sitemapPath, sitemapXml, Encoding.UTF8);
            Console.WriteLine("✅ update sitemap.xml");
        }
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

    private static DateTimeOffset? ConvertToDateTimeOffset(string output)
    {
        var dateString = output.Trim();
        string format = "yyyy-MM-ddTHH:mm:sszzz"; // 定义日期时间格式
        if (DateTimeOffset.TryParseExact(dateString, format, null, System.Globalization.DateTimeStyles.None, out var result)) { return result; }

        return null;
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
                               ⏱️ <span class="publish-time" data-time="{blog.PublishTime:yyyy-MM-ddTHH:mm:sszzz}"></span> 
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
            .DistinctBy(b => b.ToString("yyyy-MM"))
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
            var count = allBlogs.Count(b => b.PublishTime.Year == date.Year && b.PublishTime.Month == date.Month);
            var html = $"""
                <span data-date="{date:yyyy-MM}" class="filter-item text-lg block py-2 text-neutral-600 hover:text-neutral-800 dark:text-neutral-300 dark:hover:text-neutral-100">
                    {date:yyyy-MM} [{count}]
                </span>
                """;
            sb.AppendLine(html);
        }
        sb.AppendLine("</div>");

        return sb.ToString();
    }
    private string? GetTOC(string markdown)
    {
        markdown = Regex.Replace(markdown, @"```.*?```", "", RegexOptions.Singleline);
        markdown = Regex.Replace(markdown, @"`.*?`", "", RegexOptions.Singleline);

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

    private string BuildBlogPath(string path)
    {
        return BaseUrl + "blogs" + path;
    }
}
