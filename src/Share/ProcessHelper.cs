using System.Diagnostics;

namespace Share;
/// <summary>
/// 调用帮助类
/// </summary>
public class ProcessHelper
{
    /// <summary>
    /// 运行命令
    /// </summary>
    /// <param name="command"></param>
    /// <param name="args"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public static bool RunCommand(string command, string? args, out string output)
    {
        var process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        output = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (string.IsNullOrWhiteSpace(output))
        {
            output = process.StandardOutput.ReadToEnd();
            return true;
        }
        else
        {
            //Console.WriteLine($"❌ {output}");
            return false;
        }
    }
}
