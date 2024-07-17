using System.Globalization;

namespace BuildSite;
public class Language
{
    public static Dictionary<string, string> CN { get; set; } = new Dictionary<string, string>
    {
        {"Command","命令" },
        {"init","初始化配置文件webinfo.json;[path]文件路径."},
        {"build","生成静态网站;[contentPath]为内容目录,[outputPath]为网站目录路径."},
        {"buildRequired","参数[[contentPath]]与[[outputPath]]是必需的." },
        {"initSuccess","初始化配置文件成功!"},
        {"notExistWebInfo","当前目录不存在webinfo.json,将使用默认配置." }
    };
    public static Dictionary<string, string> EN { get; set; } = new Dictionary<string, string>
    {
        {"Command","Command" },
        {"init","init config file webinfo.json;[path] is path."},
        {"build","generate static website;[contentPath] is content path, [outputPath] is output path!"},
        {"buildRequired","params [[contentPath]] and [[outputPath]] is Required!" },
        {"initSuccess","Init config file Success!"},
        {"notExistWebInfo","webinfo.json not found in current path,will use default config!" }
    };


    public static string Get(string key)
    {
        var isCn = CultureInfo.CurrentCulture.Name == "zh-CN";
        return isCn ? CN[key] : EN[key];
    }
}
