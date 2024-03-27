using BuildSite;


var input = args.FirstOrDefault() ?? "./";
var output = args.Skip(1).FirstOrDefault() ?? "./_site";
var environment = args.Skip(2).FirstOrDefault() ?? "Development";


var builder = new HtmlBuilder(input, output);

try
{
    builder.BuildData();
    builder.BuildBlogs();

    if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
    {
        builder.BuildBaseHref();
    }
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.WriteLine("请尝试在根目录下运行本项目");
}
