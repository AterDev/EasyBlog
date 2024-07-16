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




    private static void LogError(string msg)
    {
        AnsiConsole.MarkupLine($"❌ [red]{msg}[/]");
    }
    private static void LogSuccess(string msg)
    {
        AnsiConsole.MarkupLine($"✅ [green]{msg}[/]");
    }
}
