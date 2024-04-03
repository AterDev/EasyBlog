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

    public string BaseHref { get; set; } = "/";
}
