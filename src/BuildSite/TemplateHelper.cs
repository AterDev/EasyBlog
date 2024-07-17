using System.Reflection;

namespace BuildSite;
internal class TemplateHelper
{
    /// <summary>
    /// 读取模板
    /// </summary>
    /// <param name="tplName"></param>
    /// <returns></returns>
    public static string GetTplContent(string tplName)
    {
        tplName = "BuildSite.template." + tplName + ".tpl";
        // 读取模板文件
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(tplName);
        if (stream == null)
        {
            Console.WriteLine("  ❌ can't find tpl file:" + tplName);
            return "";
        }
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }


    public static Stream? GetZipFileStream(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream("BuildSite.template." + fileName);

        if (stream == null)
        {
            Console.WriteLine("  ❌ can't find tpl file:" + fileName);
        }
        return stream;
    }
}
