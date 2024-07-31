using System.Text.Json;
using Models;
using Spectre.Console;

namespace BuildSite;
public class Command
{
    public static void Init(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        var filePath = Path.Combine(path, "webinfo.json");

        if (!File.Exists(filePath))
        {
            var webInfo = new WebInfo();
            var json = JsonSerializer.Serialize(webInfo, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);

        }
        LogSuccess(Language.Get("initSuccess") + "➡️" + filePath);
    }

    public static void Build(string contentPath, string outputPath)
    {
        var webInfoPath = Path.Combine("./webinfo.json");
        var webInfo = new WebInfo();
        if (File.Exists(webInfoPath))
        {
            var json = File.ReadAllText(webInfoPath);
            webInfo = JsonSerializer.Deserialize<WebInfo>(json);
        }
        else
        {
            LogInfo(Language.Get("notExistWebInfo"));
        }

        var builder = new HtmlBuilder(contentPath, outputPath, webInfo!);
        builder.BuildWebSite();
    }

    public static void LogInfo(string msg)
    {
        AnsiConsole.MarkupLine($"ℹ️ {msg}");
    }

    public static void LogError(string msg)
    {
        AnsiConsole.MarkupLine($"❌ [red]{msg}[/]");
    }
    public static void LogSuccess(string msg)
    {
        AnsiConsole.MarkupLine($"✅ [green]{msg}[/]");
    }
}
