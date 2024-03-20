namespace Models;


/// <summary>
/// 分类
/// </summary>
public class Catalog
{
    /// <summary>
    /// 名称
    /// </summary>
    public required string Name { get; set; }

    public ICollection<Catalog> Children { get; set; } = [];

    public ICollection<Blog> Blogs { get; set; } = [];

    public Catalog? Parent { get; set; }
}