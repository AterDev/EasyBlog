using BuildSite;


var input = args.FirstOrDefault() ?? "./";
var output = args.Skip(1).FirstOrDefault() ?? "./_site";

var builder = new HtmlBuilder(input, output);

try
{
    builder.BuildWebSite();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.WriteLine("请尝试在根目录下运行本项目");
}
