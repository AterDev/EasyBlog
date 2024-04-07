namespace Models;
/// <summary>
/// 站点wyth
/// </summary>
public class WebInfo
{
    /// <summary>
    /// 博客名称
    /// </summary>
    public string Name { get; set; } = BlogConst.BlogName;
    /// <summary>
    /// 博客描述
    /// </summary>
    public string Description { get; set; } = BlogConst.BlogDescription;

    /// <summary>
    /// 作者名称
    /// </summary>
    public string AuthorName { get; set; } = "Ater";

    /// <summary>
    /// 子目录，无则保持"/"
    /// </summary>
    public string BaseHref { get; set; } = "/";

    /// <summary>
    /// 部署时的域名,用于生成 sitemap.xml
    /// 例如:https://aterdev.github.io或https://blog.dusi.dev
    /// </summary>
    public string? Domain { get; set; }
}
