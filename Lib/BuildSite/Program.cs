using BuildSite;


var input = args[0] ?? "./";
var output = args[1] ?? "./_site";
var environment = args[2] ?? "Development";


var builder = new HtmlBuilder(input, output);

try
{
    builder.BuildBlogs();
    builder.BuildData();


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
