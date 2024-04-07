using System.Xml.Linq;

namespace Models;
public class Sitemap
{
    public required string Loc { get; set; }
    public required string Lastmod { get; set; }
    public string Changefreq { get; set; } = "daily";
    public string Priority { get; set; } = "0.9";

    /// <summary>
    /// 生成sitemap.xml
    /// </summary>
    /// <param name="sitemaps"></param>
    /// <returns></returns>
    public static string GetSitemaps(List<Sitemap> sitemaps)
    {
        XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var xml = new XDocument(new XDeclaration("1.0", "utf-8", null),
            new XElement(xmlns + "urlset",
                from sitemap in sitemaps
                select
                    new XElement(xmlns + "url",
                        new XElement(xmlns + "loc", sitemap.Loc),
                        new XElement(xmlns + "lastmod", sitemap.Lastmod),
                        new XElement(xmlns + "changefreq", sitemap.Changefreq),
                        new XElement(xmlns + "priority", sitemap.Priority)
                    )
                )
            );
        // return xml as string
        var declaration = xml.Declaration?.ToString();
        return declaration + Environment.NewLine + xml.ToString();
    }
}
