using System.Collections.Generic;
using ColorCode.Core;
using ColorCode.Core.Common;

namespace ColorCode.Core.Compilation.Languages;
public class Bash : ILanguage
{
    public string Id
    {
        get { return LanguageId.Bash; }
    }
    public string FirstLinePattern
    {
        get { return null; }
    }
    public string Name { get { return "Bash"; } }
    public string CssClassName { get { return "bash"; } }
    public IList<LanguageRule> Rules
    {
        get
        {
            return [
                new LanguageRule(@"(?i)\b(if|then|elif|else|fi|case|esac|for|do|while|until|function|return|break|continue|sudo|scp)\b",
                    new Dictionary<int, string>{
                        { 1, ScopeName.Keyword }
                    }),
                new LanguageRule(@"(\#.*?)\r?$",
                    new Dictionary<int, string>
                        {
                            {1, ScopeName.Comment}
                        }),
                new LanguageRule(@"'[^\n]*?(?<!\\)'",
                    new Dictionary<int, string>
                        {
                            {0, ScopeName.String}
                        }),
                new LanguageRule(@"(?s)(""[^\n]*?(?<!`)"")",
                    new Dictionary<int, string>
                        {
                            {0, ScopeName.String}
                        }),
                new LanguageRule(@"-\w+\d*\w*",
                    new Dictionary<int, string>
                        {
                            {0, ScopeName.PowerShellParameter}
                        }
                    ),
                new LanguageRule(@"\b(\w+\-\w+)\b",
                    new Dictionary<int, string>
                        {
                            {1, ScopeName.PowerShellCommand}
                        }),

                ];
        }
    }

    public bool HasAlias(string lang)
    {
        return lang.ToLower() switch
        {
            "bash" or "sh" => true,
            _ => false,
        };
    }

    public override string ToString()
    {
        return Name;
    }
}
