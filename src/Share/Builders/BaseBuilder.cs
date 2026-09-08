using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Share.MarkdownExtension;

namespace Share.Builders;

public partial class BaseBuilder
{
    public WebInfo WebInfo { get; init; }

    public string ContentPath { get; init; }
    public string Output { get; init; }
    public string DataPath { get; init; }

    public string BaseUrl { get; set; }

    public static Dictionary<string, string> DocMenus { get; } = new(StringComparer.OrdinalIgnoreCase);
    public static Dictionary<string, string> ProductMenus { get; } = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, (DateTimeOffset? Created, DateTimeOffset? Updated, string? CreatedAuthor, string? UpdatedAuthor)> GitTimeCache = new(StringComparer.OrdinalIgnoreCase);
    private static bool _isGitLoaded = false;
    private static readonly object _gitLoadLock = new();

    public BaseBuilder(WebInfo webInfo)
    {
        WebInfo = webInfo;
        BaseUrl = "/";
        ContentPath = webInfo.ContetPath.EndsWith(Path.DirectorySeparatorChar) ? webInfo.ContetPath[0..^1] : webInfo.ContetPath;
        Output = webInfo.OutputPath;
        DataPath = Path.Combine(Output, BlogConst.DataPath);
    }

    public static void ResetMenus()
    {
        DocMenus.Clear();
        ProductMenus.Clear();
        lock (_gitLoadLock)
        {
            GitTimeCache.Clear();
            _isGitLoaded = false;
        }
    }

    public void EnableBaseUrl()
    {
        BaseUrl = WebInfo?.BaseHref ?? "/";
        if (!BaseUrl.EndsWith('/'))
        {
            BaseUrl += "/";
        }
    }

    protected string BuildCanonicalUrl(string? relativePath)
    {
        var basePath = BaseUrl;
        if (!basePath.StartsWith('/'))
        {
            basePath = "/" + basePath;
        }
        if (!basePath.EndsWith('/'))
        {
            basePath += "/";
        }

        var normalized = (relativePath ?? string.Empty).Replace("\\", "/").TrimStart('/');
        var path = string.IsNullOrWhiteSpace(normalized) ? basePath : $"{basePath}{normalized}";

        if (!string.IsNullOrWhiteSpace(WebInfo?.Domain))
        {
            var domain = WebInfo.Domain.TrimEnd('/');
            return path.StartsWith('/') ? domain + path : domain + "/" + path;
        }

        return path;
    }

    protected string BuildSiteUrl(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        return BaseUrl.TrimEnd('/') + "/" + normalized;
    }

    /// <summary>
    /// 规范化仓库网页地址，避免将 Git remote 中的凭据写入生成的站点。
    /// </summary>
    protected static string? NormalizeRepositoryUrl(string? repositoryUrl)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
        {
            return null;
        }

        var normalized = repositoryUrl.Trim();
        if (normalized.StartsWith("git@", StringComparison.OrdinalIgnoreCase))
        {
            var separator = normalized.IndexOf(':', 4);
            if (separator <= 4 || separator == normalized.Length - 1)
            {
                return null;
            }

            normalized = $"https://{normalized[4..separator]}/{normalized[(separator + 1)..]}";
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri) ||
            string.IsNullOrWhiteSpace(uri.Host))
        {
            return null;
        }

        var builder = new UriBuilder(uri)
        {
            UserName = string.Empty,
            Password = string.Empty,
            Query = string.Empty,
            Fragment = string.Empty
        };

        var path = builder.Path.TrimEnd('/');
        if (path.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            path = path[..^4].TrimEnd('/');
        }

        builder.Path = path;
        return builder.Uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
    }

    protected string GetPageKeywords(string? title = null)
    {
        if (!string.IsNullOrWhiteSpace(WebInfo?.Keywords))
        {
            return WebInfo!.Keywords!;
        }

        var name = WebInfo?.Name ?? string.Empty;
        var author = WebInfo?.AuthorName ?? string.Empty;
        var titlePart = string.IsNullOrWhiteSpace(title) ? string.Empty : $"{title},";
        var blogKeyword = WebInfo?.EnableBlog != false ? ",blog" : string.Empty;
        return $"{titlePart}{name},{author}{blogKeyword},docs,documentation";
    }

    protected string GetFooterText()
    {
        if (!string.IsNullOrWhiteSpace(WebInfo.FooterText))
        {
            return WebInfo.FooterText!;
        }

        return $"{WebInfo.Name}\n<a class=\"footer-link\" target=\"_blank\" href=\"https://github.com/AterDev/EasyBlog\">Powered by EasyDocs</a>";
    }

    /// <summary>
    /// 内容页TOC
    /// </summary>
    /// <param name="markdown"></param>
    /// <returns></returns>
    protected string? GetContentTOC(string markdown)
    {
        MatchCollection matches = GetHeading2Matches(markdown);

        if (matches.Count > 0)
        {
            var tocBuilder = new StringBuilder();
            tocBuilder.AppendLine("<div class=\"toc-block toc-sticky\">");
            tocBuilder.AppendLine(" <p class=\"toc-title\">In this article</p>");
            tocBuilder.AppendLine(@"<ul class=""toc"">");

            foreach (Match match in matches)
            {
                string headingText = match.Groups[1].Value.Trim();
                string headingId = NormalizeGitHub(headingText);

                // 去除表情符号
                headingId = Regex.Replace(headingId, @"[\uD800-\uDBFF][\uDC00-\uDFFF]", "");

                var encodedHeadingId = System.Net.WebUtility.HtmlEncode(headingId);
                var encodedHeadingText = System.Net.WebUtility.HtmlEncode(headingText);

                tocBuilder.AppendLine($"""
                    <li>
                      <a href="#{encodedHeadingId}">{encodedHeadingText}</a>
                    </li>
                    """);
            }
            tocBuilder.AppendLine("</ul>");
            tocBuilder.AppendLine("</div>");
            return tocBuilder.ToString();
        }
        return null;
    }

    protected List<string> GetContentHeading2(string markdown)
    {
        var headings = new List<string>();
        MatchCollection matches = GetHeading2Matches(markdown);
        foreach (Match match in matches)
        {
            var text = match.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                headings.Add(text);
            }
        }

        return headings;
    }

    private static MatchCollection GetHeading2Matches(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return Regex.Matches(string.Empty, "$a");
        }

        markdown = Regex.Replace(markdown, @"```.*?```", "", RegexOptions.Singleline);
        markdown = Regex.Replace(markdown, @"`.*?`", "", RegexOptions.Singleline);

        string heading2Pattern = @"^##\s+(.+)$";
        return Regex.Matches(markdown, heading2Pattern, RegexOptions.Multiline);
    }

    /// <summary>
    /// 获取标题
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    protected static string GetTitleFromMarkdown(string content)
    {
        // 使用正则表达式匹配标题
        var regex = TitleRegex();
        var match = regex.Match(content);
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }

    private static string GetFullPath(Catalog catalog)
    {
        var path = catalog.Name;
        if (catalog.Parent != null)
        {
            path = Path.Combine(GetFullPath(catalog.Parent), path);
        }
        return path.Replace("Root", "");
    }
    protected string GetExtensionScript(string content)
    {
        string extensionHead = "";
        if (content.Contains("class=\"math\""))
        {
            extensionHead += """
                <script src="https://polyfill.io/v3/polyfill.min.js?features=es6"></script>
                <script id="MathJax-script" async src="https://cdn.jsdelivr.net/npm/mathjax@3.0.1/es5/tex-mml-chtml.js"></script>
                
                """;
        }
        if (content.Contains("class=\"nomnoml\""))
        {
            extensionHead += """
                <script src="//unpkg.com/graphre/dist/graphre.js"></script>
                <script src="//unpkg.com/nomnoml/dist/nomnoml.js"></script>
                """;
        }
        return extensionHead;
    }

    protected string BuildMarkdownContent(Doc doc)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAlertBlocks()
            .UseFigures()
            .UseCitations()
            .UseEmphasisExtras()
            .UseMathematics()
            .UseMediaLinks()
            .UseListExtras()
            .UseTaskLists()
            .UseDiagrams()
            .UseAutoLinks()
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .UsePipeTables()
            .UseBetterCodeBlock()
            .UseLinkConvert()
            .Build();

        var markdown = File.ReadAllText(doc.Path);
        return Markdown.ToHtml(markdown, pipeline);
    }

    protected void CopyStaticFiles(string sourceRoot, string outputRoot, Func<string, bool>? predicate = null)
    {
        if (!Directory.Exists(sourceRoot))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(file);
            if (fileName.Equals(".order", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ||
                predicate != null && !predicate(file))
            {
                continue;
            }

            var relativePath = Path.GetRelativePath(sourceRoot, file);
            var targetPath = Path.Combine(outputRoot, relativePath);
            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(file, targetPath, true);
        }
    }

    protected string? FindAboutFile(string contentPath)
    {
        if (!Directory.Exists(contentPath))
        {
            return null;
        }

        var candidates = Directory.EnumerateFiles(contentPath, "*.md", SearchOption.TopDirectoryOnly)
            .Where(path => Path.GetFileName(path).Equals("about.md", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (candidates.Count > 1)
        {
            throw new InvalidOperationException("Only one about.md/About.md file is allowed.");
        }

        return candidates.SingleOrDefault();
    }

    /// <summary>
    /// 递归构建Catalog
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <param name="parentCatalog"></param>
    protected void TraverseDirectory(string directoryPath, Catalog parentCatalog)
    {
        // 确保 Git 历史已加载
        LoadGitHistory(directoryPath);

        // 排序数据
        var orderFile = Path.Combine(directoryPath, ".order");
        string[] orderData = [];
        if (File.Exists(orderFile))
        {
            orderData = File.ReadAllLines(orderFile).Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();
        }

        // 目录及文件
        var dirsPath = Directory.GetDirectories(directoryPath);
        var filesPath = Directory.GetFiles(directoryPath, "*.md");
        var allPath = dirsPath.Concat(filesPath).ToArray();

        if (allPath.Length > 0)
        {
            var orderedPaths = new List<string>();
            if (orderData.Length > 0)
            {
                foreach (var file in orderData)
                {
                    var path = allPath.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == file);
                    if (path != null)
                    {
                        orderedPaths.Add(path);
                    }
                }
                var unorderedFiles = allPath.Except(orderedPaths);
                orderedPaths.AddRange(unorderedFiles);
            }
            else
            {
                orderedPaths = allPath.ToList();
            }

            foreach (string itemPath in orderedPaths)
            {
                if (File.Exists(itemPath))
                {
                    var fileInfo = new FileInfo(itemPath);
                    var fileName = Path.GetFileName(itemPath);
                    var gitAddTime = GetCreatedTime(itemPath);
                    var gitUpdateTime = GetUpdatedTime(itemPath);
                    var gitAuthor = GetUpdatedAuthor(itemPath);
                    var doc = new Doc
                    {
                        Title = Path.GetFileNameWithoutExtension(itemPath),
                        FileName = fileName,
                        Path = itemPath,
                        PublishTime = gitUpdateTime ?? gitAddTime ?? fileInfo.LastWriteTime,
                        CreatedTime = gitAddTime ?? fileInfo.CreationTime,
                        UpdatedTime = gitUpdateTime ?? fileInfo.LastWriteTime,
                        AuthorName = string.IsNullOrWhiteSpace(gitAuthor) ? WebInfo.AuthorName : gitAuthor,
                        Catalog = parentCatalog
                    };

                    doc.HtmlPath = Path.Combine(GetFullPath(parentCatalog), doc.FileName.Replace(".md", ".html"));

                    doc.HtmlPath = doc.HtmlPath.Replace('\\', '/');
                    parentCatalog.Docs.Add(doc);
                }
                else if (Directory.Exists(itemPath))
                {
                    var existMd = Directory.GetFiles(itemPath, "*.md").Length > 0;
                    var existDir = Directory.GetDirectories(itemPath).Length > 0;
                    var name = Path.GetFileName(itemPath);
                    if (existMd || existDir && name != "_images")
                    {
                        var catalog = new Catalog
                        {
                            Name = name,
                            Parent = parentCatalog,
                            Path = itemPath
                        };
                        parentCatalog.Children.Add(catalog);
                        TraverseDirectory(itemPath, catalog);
                    }
                }
            }
        }
    }

    protected string BuildCatalogTree(Catalog rootCatalog, string routePrefix)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<div class=\"tree\">");
        sb.AppendLine("<ul class=\"root-list\">");
        GenerateCatalogHtml(rootCatalog, routePrefix, sb);
        sb.AppendLine("</ul>");
        sb.AppendLine("</div>");
        return sb.ToString();
    }

    protected List<Doc> GetOrderedDocs(Catalog catalog)
    {
        var docs = new List<Doc>();
        foreach (var item in GetOrderedCatalogItems(catalog))
        {
            if (item.Doc != null)
            {
                docs.Add(item.Doc);
            }
            else if (item.Catalog != null)
            {
                docs.AddRange(GetOrderedDocs(item.Catalog));
            }
        }

        return docs;
    }

    private void GenerateCatalogHtml(Catalog catalog, string routePrefix, StringBuilder sb)
    {
        foreach (var item in GetOrderedCatalogItems(catalog))
        {
            if (item.Doc != null)
            {
                var href = BuildSiteUrl(routePrefix + "/" + item.Doc.HtmlPath);
                var displayName = Path.GetFileNameWithoutExtension(item.Doc.FileName);
                var id = ComputeMD5Hash(item.Doc.HtmlPath);
                sb.AppendLine("<li data-doc-id=\"" + id + "\" class=\"space\">");
                sb.AppendLine("<a class=\"text\" href=\"" + href + "\">" +
                    System.Net.WebUtility.HtmlEncode(displayName) + "</a>");
                sb.AppendLine("</li>");
                continue;
            }

            if (item.Catalog != null)
            {
                sb.AppendLine("<li><span class=\"caret\">" +
                    System.Net.WebUtility.HtmlEncode(item.Catalog.Name) + "</span>");
                sb.AppendLine("<ul class=\"nested\">");
                GenerateCatalogHtml(item.Catalog, routePrefix, sb);
                sb.AppendLine("</ul>");
                sb.AppendLine("</li>");
            }
        }
    }

    private static IEnumerable<(Doc? Doc, Catalog? Catalog)> GetOrderedCatalogItems(Catalog catalog)
    {
        var docs = catalog.Docs.ToDictionary(
            doc => Path.GetFileNameWithoutExtension(doc.FileName),
            StringComparer.OrdinalIgnoreCase);
        var children = catalog.Children.ToDictionary(child => child.Name, StringComparer.OrdinalIgnoreCase);
        var consumedDocs = new HashSet<Doc>();
        var consumedChildren = new HashSet<Catalog>();
        var orderPath = Path.Combine(catalog.Path, ".order");

        if (File.Exists(orderPath))
        {
            foreach (var entry in File.ReadLines(orderPath).Select(line => line.Trim()).Where(line => line.Length > 0))
            {
                if (docs.TryGetValue(entry, out var doc))
                {
                    consumedDocs.Add(doc);
                    yield return (doc, null);
                }
                else if (children.TryGetValue(entry, out var child))
                {
                    consumedChildren.Add(child);
                    yield return (null, child);
                }
            }
        }

        foreach (var doc in catalog.Docs)
        {
            if (consumedDocs.Add(doc))
            {
                yield return (doc, null);
            }
        }

        foreach (var child in catalog.Children)
        {
            if (consumedChildren.Add(child))
            {
                yield return (null, child);
            }
        }
    }

    /// <summary>
    /// 菜单导航
    /// </summary>
    /// <returns></returns>
    protected string BuildNavigations(string contentPath)
    {
        var hasDocs = DocMenus.Count > 0;
        var hasProducts = ProductMenus.Count > 0;
        var hasBlog = WebInfo?.EnableBlog == true && Directory.Exists(Path.Combine(contentPath, "blogs"));
        var hasAbout = FindAboutFile(contentPath) != null;
        var navigations = new StringBuilder();
        if (hasBlog)
        {
            navigations.AppendLine("<a href=\"" + BuildSiteUrl("blogs.html") + "\" class=\"nav-link\">Blogs</a>");
        }
        if (hasDocs)
        {
            var docLinkHtml = "";
            foreach (var menu in DocMenus)
            {
                docLinkHtml += "<a href=\"" + BuildSiteUrl("docs/" + menu.Value) + "\" class=\"dropdown-item\">" + System.Net.WebUtility.HtmlEncode(menu.Key) + "</a>" + Environment.NewLine;
            }
                        var docsMenuHtml = $$"""
                                <div class="dropdown">
                                    <div>
                                        <button type="button" class="dropdown-toggle nav-link">
                                            Docs
                                            <svg class="dropdown-icon" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true"
                                                data-slot="icon">
                                                <path fill-rule="evenodd"
                                                    d="M5.22 8.22a.75.75 0 0 1 1.06 0L10 11.94l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L5.22 9.28a.75.75 0 0 1 0-1.06Z"
                                                    clip-rule="evenodd" />
                                            </svg>
                                        </button>
                                    </div>
                                    <div class="dropdown-menu" tabindex="-1">
                                        <div role="none">
                                            {{docLinkHtml}}
                                        </div>
                                    </div>
                                </div>
                                """;
            navigations.AppendLine(docsMenuHtml);
        }
        if (hasProducts)
        {
            navigations.AppendLine(BuildNavigationDropdown("Products", ProductMenus, "products"));
        }
        if (hasAbout)
        {
            navigations.AppendLine("<a href=\"" + BuildSiteUrl("about.html") + "\" target=\"_blank\" class=\"nav-link\">About</a>");
        }
        return navigations.ToString();
    }

    private string BuildNavigationDropdown(string title, Dictionary<string, string> menus, string routePrefix)
    {
        var links = new StringBuilder();
        foreach (var menu in menus)
        {
            links.AppendLine("<a href=\"" + BuildSiteUrl(routePrefix + "/" + menu.Value) +
                "\" class=\"dropdown-item\">" +
                System.Net.WebUtility.HtmlEncode(menu.Key) + "</a>");
        }

        var dropdownArrow = """
            <svg class="dropdown-icon" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true"
                data-slot="icon">
                <path fill-rule="evenodd"
                    d="M5.22 8.22a.75.75 0 0 1 1.06 0L10 11.94l3.72-3.72a.75.75 0 1 1 1.06 1.06l-4.25 4.25a.75.75 0 0 1-1.06 0L5.22 9.28a.75.75 0 0 1 0-1.06Z"
                    clip-rule="evenodd" />
            </svg>
            """;

        return "<div class=\"dropdown\">" + Environment.NewLine +
            "  <div><button type=\"button\" class=\"dropdown-toggle nav-link\">" +
            System.Net.WebUtility.HtmlEncode(title) + Environment.NewLine +
            dropdownArrow + Environment.NewLine +
            "  </button></div>" + Environment.NewLine +
            "  <div class=\"dropdown-menu\" tabindex=\"-1\"><div role=\"none\">" +
            Environment.NewLine + links +
            "  </div></div>" + Environment.NewLine +
            "</div>";
    }

    private static void LoadGitHistory(string directoryPath)
    {
        if (_isGitLoaded) return;
        lock (_gitLoadLock)
        {
            if (_isGitLoaded) return;

            try
            {
                // 获取 Git 根目录
                if (!ProcessHelper.RunCommand("git", "rev-parse --show-toplevel", out string gitRoot, directoryPath))
                {
                    _isGitLoaded = true; // Git 不可用，跳过
                    return;
                }
                gitRoot = gitRoot.Trim().Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

                var process = new Process();
                process.StartInfo.FileName = "git";
                // 获取所有提交日志，格式：COMMIT_DATE:ISO8601 和 COMMIT_AUTHOR:name
                // 紧接着是文件状态和路径
                // 增加 -c core.quotepath=false 防止中文路径被转义
                process.StartInfo.Arguments = "-c core.quotepath=false log --name-status --date=iso-strict --format=\"COMMIT_DATE:%ad%nCOMMIT_AUTHOR:%an\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WorkingDirectory = gitRoot; // 在 Git 根目录运行
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

                process.Start();

                string? line;
                DateTimeOffset currentCommitDate = DateTimeOffset.MinValue;
                string? currentCommitAuthor = null;

                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("COMMIT_DATE:"))
                    {
                        if (DateTimeOffset.TryParse(line.Substring(12), out var date))
                        {
                            currentCommitDate = date;
                        }
                        continue;
                    }

                    if (line.StartsWith("COMMIT_AUTHOR:", StringComparison.Ordinal))
                    {
                        currentCommitAuthor = line[14..].Trim();
                        continue;
                    }

                    // 解析状态和路径
                    // 格式: M    path/to/file
                    // 格式: A    path/to/file
                    // 格式: R100 old/path new/path
                    var parts = line.Split('\t');
                    if (parts.Length >= 2)
                    {
                        var status = parts[0][0];
                        var filePath = parts.Last(); // 对于重命名，取新路径

                        // 转换为本地路径
                        var fullPath = Path.GetFullPath(Path.Combine(gitRoot, filePath.Replace('/', Path.DirectorySeparatorChar)));

                        if (!GitTimeCache.TryGetValue(fullPath, out var info))
                        {
                            // 第一次遇到文件（从新到旧），这是最后修改时间
                            GitTimeCache[fullPath] = (null, currentCommitDate, null, currentCommitAuthor);
                        }

                        // 如果是添加操作，更新创建时间（从新到旧扫描，越旧的 A 越接近真实创建时间）
                        if (status == 'A')
                        {
                            var currentInfo = GitTimeCache[fullPath];
                            // 只有当 Created 为空时才设置，或者我们总是取最新的 'A'？
                            // git log --diff-filter=A 返回的是包含 A 的 commit。
                            // 如果文件被删除重加，最新的 A 是最近一次添加。
                            // 我们希望 CreatedTime 是最近一次添加的时间。
                            // 因为我们是从新到旧扫描，遇到的第一个 A 就是最近的 A。
                            if (!currentInfo.Created.HasValue)
                            {
                                currentInfo.Created = currentCommitDate;
                                currentInfo.CreatedAuthor = currentCommitAuthor;
                                GitTimeCache[fullPath] = currentInfo;
                            }
                        }
                    }
                }
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] LoadGitHistory failed: {ex.Message}");
            }
            finally
            {
                _isGitLoaded = true;
            }
        }
    }

    private static DateTimeOffset? GetCreatedTime(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (GitTimeCache.TryGetValue(fullPath, out var info) && info.Created.HasValue)
        {
            return info.Created;
        }

        if (_isGitLoaded) return null;

        if (ProcessHelper.RunCommand("git", @$"log --diff-filter=A --format=%aI -- ""{path}""", out string output))
        {
            output = output.Split("\n").First();
            return ConvertToDateTimeOffset(output);
        }
        return null;
    }

    private static DateTimeOffset? GetUpdatedTime(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (GitTimeCache.TryGetValue(fullPath, out var info) && info.Updated.HasValue)
        {
            return info.Updated;
        }

        if (_isGitLoaded) return null;

        return ProcessHelper.RunCommand("git", @$"log -n 1 --format=%aI -- ""{path}""", out string output)
            ? ConvertToDateTimeOffset(output)
            : null;
    }

    private static string? GetUpdatedAuthor(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (GitTimeCache.TryGetValue(fullPath, out var info))
        {
            return string.IsNullOrWhiteSpace(info.UpdatedAuthor) ? info.CreatedAuthor : info.UpdatedAuthor;
        }

        return ProcessHelper.RunCommand("git", @$"log -n 1 --format=%an -- ""{path}""", out string output)
            ? output.Trim()
            : null;
    }

    private static DateTimeOffset? ConvertToDateTimeOffset(string output)
    {
        var dateString = output.Trim();
        string format = "yyyy-MM-ddTHH:mm:sszzz"; // 定义日期时间格式
        return DateTimeOffset.TryParseExact(dateString, format, null, System.Globalization.DateTimeStyles.None, out var result)
            ? result
            : null;
    }


    public static string ComputeMD5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }

    [GeneratedRegex(@"^# (.*)$", RegexOptions.Multiline)]
    private static partial Regex TitleRegex();

    private static string NormalizeGitHub(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var builder = new StringBuilder(text.Length);
        bool previousIsDash = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
            {
                if (!previousIsDash)
                {
                    builder.Append('-');
                    previousIsDash = true;
                }
            }
            else if (IsValidAnchorChar(c))
            {
                builder.Append(char.ToLowerInvariant(c));
                previousIsDash = false;
            }
        }
        // Remove trailing dash
        if (builder.Length > 0 && builder[builder.Length - 1] == '-')
            builder.Length--;
        return builder.ToString();
    }

    private static bool IsValidAnchorChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '-' || c == '_';
    }
}
