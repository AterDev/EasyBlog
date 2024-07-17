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
        var contentPath = args.Skip(1).FirstOrDefault();
        var outputPath = args.Skip(2).FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(contentPath) && !string.IsNullOrWhiteSpace(outputPath))
        {
            Command.Build(contentPath, outputPath);
        }
        else
        {
            Command.LogError(Language.Get("buildRequired"));
        }
        break;
    default:
        ShowHelp();
        break;
}

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
            EasyBlog : The Static Web Builder!
               —→ for freedom 🗽 ←—

            """;

    Console.WriteLine(logo);
}
