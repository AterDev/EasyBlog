using System.Collections.Generic;
using ColorCode.Common;

namespace ColorCode.Compilation.Languages;
public class SimpleText : ILanguage
{
    public string Id
    {
        get { return LanguageId.SimpleText; }
    }

    public string Name
    {
        get { return "SimpleText"; }
    }

    public string CssClassName
    {
        get { return "plaintext"; }
    }

    public string FirstLinePattern
    {
        get { return null; }
    }

    public IList<LanguageRule> Rules
    {
        get
        {
            return [

                new LanguageRule(@"^\s*\b(?:\w+|if|location|server|http)\b(?=\s)",
                    new Dictionary<int, string>
                    {
                        { 0, ScopeName.Keyword },
                    }),
                new LanguageRule(@"^\s*#.*$",
                    new Dictionary<int, string>
                    {
                        { 1, ScopeName.Comment },
                    }),

                new LanguageRule(@"(\$[a-zA-Z_][a-zA-Z0-9_-]*)",
                    new Dictionary<int, string>
                    {
                        { 1, ScopeName.Variable },
                    }),

                new LanguageRule(@"([""']).*?\1",
                    new Dictionary<int, string>
                    {
                        { 1, ScopeName.String },
                    }),

                new LanguageRule(@"[\(\)\[\]\{\}]",
                    new Dictionary<int, string>
                        {
                            { 0, ScopeName.ControlKeyword },
                        }),
                ];
        }
    }

    public bool HasAlias(string lang)
    {
        return lang.ToLower() switch
        {
            "nginx" or "conf" or "" => true,
            _ => false,
        };
    }

    public override string ToString()
    {
        return Name;
    }
}
