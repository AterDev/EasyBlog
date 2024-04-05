using System.Collections.Generic;
using ColorCode.Common;

namespace ColorCode.Compilation.Languages;
public class Ini : ILanguage
{
    public string Id
    {
        get { return LanguageId.Ini; }
    }
    public string FirstLinePattern
    {
        get { return null; }
    }
    public string Name { get { return "INI"; } }
    public string CssClassName { get { return "ini"; } }
    public IList<LanguageRule> Rules
    {
        get
        {
            return [
                new LanguageRule(@"^(\w+)\s*=",
                    new Dictionary<int, string>{
                        { 1, ScopeName.Keyword }
                    }),
                new LanguageRule(@"(\#.*?)\r?$",
                    new Dictionary<int, string>
                        {
                            {1, ScopeName.Comment}
                        }),
                new LanguageRule(@"^=\s*(.*)$",
                    new Dictionary<int, string>
                        {
                            {0, ScopeName.String}
                        }),
                new LanguageRule(@"\[(\w+)\]\s*",
                    new Dictionary<int, string>
                        {
                            {0, ScopeName.NameSpace}
                        })
                ];
        }
    }

    public bool HasAlias(string lang)
    {
        return lang.ToLower() switch
        {
            "ini" or "toml" => true,
            _ => false,
        };
    }

    public override string ToString()
    {
        return Name;
    }
}
