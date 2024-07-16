using BuildSite;
using Spectre.Console;


ShowLogo();
string? command = args.FirstOrDefault();

switch (command)
{
    case "init":
        var path = args.Skip(1).FirstOrDefault() ?? Directory.GetCurrentDirectory();

        Command.Init(path);
        break;

    case "build":
        break;
    default:
        ShowHelp();
        break;
}


//var environment = args.Skip(2).FirstOrDefault() ?? "Development";

//if (!string.IsNullOrWhiteSpace(input) && !string.IsNullOrWhiteSpace(output))
//{
//    var builder = new HtmlBuilder(input, output);

//    try
//    {
//        if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
//        {
//            builder.EnableBaseUrl();
//        }

//        builder.BuildWebSite();
//    }
//    catch (Exception e)
//    {
//        Console.WriteLine(e.Message + e.StackTrace);
//        Console.WriteLine("请尝试在根目录下运行本项目");
//    }
//}
static void ShowHelp()
{
    var helpContent = """

    {0}:
    easyblog init [path]
        {1}

    easyblog build [contentPath] [outputPath]
        {2}
    """;
    AnsiConsole.Write(helpContent,
        Language.Get("Command"),
        Language.Get("init"),
        Language.Get("build")
        );
}
static void ShowLogo()
{
    var logo = """
            EasyBlog
            The Static Web Builder!

               —→ for freedom 🗽 ←—
            """;

    Console.WriteLine(logo);
}
