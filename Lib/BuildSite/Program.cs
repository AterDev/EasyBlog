using BuildSite;

var builder = new HtmlBuilder();
builder.BuildBlogs();
builder.BuildData();

if (args.Length > 0 && args[0].Equals("production", StringComparison.OrdinalIgnoreCase))
{
    builder.BuildBaseHref();
}