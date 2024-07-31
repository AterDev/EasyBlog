using System.Collections.Generic;
using ColorCode.Core.Common;

namespace ColorCode.Core.Compilation.Languages;
public class Yml : ILanguage
{
    public string Id
    {
        get { return LanguageId.Yml; }
    }
    public string FirstLinePattern
    {
        get { return null; }
    }
    public string Name { get { return "YML"; } }
    public string CssClassName { get { return "yml"; } }
    public IList<LanguageRule> Rules
    {
        get
        {
            return [
                new LanguageRule(@"^\s*([a-zA-Z0-9_-]+)\s*:",
                    new Dictionary<int, string>{
                        { 1, ScopeName.Keyword }
                    }),
                new LanguageRule(@"""([^""\\]*(?:\\.[^""\\]*)*)""|'([^'\\]*(?:\\.[^'\\]*)*)'",
                    new Dictionary<int, string>{
                        { 1, ScopeName.String }
                    }),
                new LanguageRule(@"^\s*#.*",
                    new Dictionary<int, string>{
                        { 1, ScopeName.Comment }
                    }),
                ];
        }
    }

    public bool HasAlias(string lang)
    {
        return lang.ToLower() switch
        {
            "yml" or "yaml" => true,
            _ => false,
        };
    }

    public override string ToString()
    {
        return Name;
    }
}
