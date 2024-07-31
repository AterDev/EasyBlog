using System.Collections.Generic;
using ColorCode.Core;
using ColorCode.Core.Common;

namespace ColorCode.Core.Compilation.Languages;
public class Docker : ILanguage
{
    public string Id
    {
        get { return LanguageId.Docker; }
    }
    public string FirstLinePattern
    {
        get { return null; }
    }
    public string Name { get { return "Docker"; } }
    public string CssClassName { get { return "docker"; } }
    public IList<LanguageRule> Rules
    {
        get
        {
            return [

                new LanguageRule(@"(?i)\b(FROM|MAINTAINER|RUN|CMD|LABEL|EXPOSE|ENV|ADD|COPY|ENTRYPOINT|VOLUME|USER|WORKDIR|ARG|ONBUILD)\b",
                    new Dictionary<int, string>{
                        { 1, ScopeName.Keyword }
                    }),
                new LanguageRule(@"""([^""\\]*(?:\\.[^""\\]*)*)""|'([^'\\]*(?:\\.[^'\\]*)*)'",
                    new Dictionary<int, string>{
                        { 1, ScopeName.String }
                    }),

                new LanguageRule(@"\b[0-9]{1,}\b",
                        new Dictionary<int, string>
                            {
                                { 0, ScopeName.Number }
                            }),
                new LanguageRule(@"^#[^\n]*",
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
            "docker" => true,
            _ => false,
        };
    }

    public override string ToString()
    {
        return Name;
    }
}
