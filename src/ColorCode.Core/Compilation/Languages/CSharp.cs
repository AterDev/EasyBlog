// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using ColorCode.Common;

namespace ColorCode.Compilation.Languages;

public class CSharp : ILanguage
{
    public string Id
    {
        get { return LanguageId.CSharp; }
    }

    public string Name
    {
        get { return "C#"; }
    }

    public string CssClassName
    {
        get { return "csharp"; }
    }

    public string FirstLinePattern
    {
        get
        {
            return null;
        }
    }

    public IList<LanguageRule> Rules
    {
        get
        {
            return [
                new LanguageRule(
                    @"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/",
                    new Dictionary<int, string>
                        {
                            { 0, ScopeName.Comment },
                        }),
                new LanguageRule(
                    @"(///)(?:\s*?(<[/a-zA-Z0-9\s""=]+>))*([^\r\n]*)",
                    new Dictionary<int, string>
                        {
                            { 1, ScopeName.XmlDocTag },
                            { 2, ScopeName.XmlDocTag },
                            { 3, ScopeName.XmlDocComment },
                        }),
                new LanguageRule(
                    @"(//.*?)\r?$",
                    new Dictionary<int, string>
                        {
                            { 1, ScopeName.Comment }
                        }),

                new LanguageRule(
                    @"(?s)@""(?:""""|.)*?""(?!"")",
                    new Dictionary<int, string>
                        {
                            { 0, ScopeName.StringCSharpVerbatim }
                        }),

                new LanguageRule(@"\b(class|struct|enum|interface)\s+(\w+)\b",
                    new Dictionary<int, string>
                    {
                        { 1, ScopeName.Keyword },
                        { 2, ScopeName.ClassName }
                    }),

                new LanguageRule(@"[\(\)\[\]\{\}]",
                    new Dictionary<int, string>
                        {
                            { 0, ScopeName.ControlKeyword },
                        }),
                new LanguageRule(@"\b([A-Z][a-z0-9A-Z]*)<([A-Z][a-z0-9A-Z]*)>",
                    new Dictionary<int, string>
                        {
                            { 1, ScopeName.ClassName },
                            { 2, ScopeName.Symbol},
                        }),
                new LanguageRule(@" +\b([A-Z][a-z0-9A-Z]*) +([a-z][a-z0-9A-Z]*)\b +=",
                    new Dictionary<int, string>
                        {
                            { 1, ScopeName.ClassName },
                            { 2, ScopeName.Variable},
                        }),
                new LanguageRule(@"([A-Z][a-z0-9A-Z]*)\.([A-Z][a-z0-9A-Z]*)[; +]",
                    new Dictionary<int, string>
                        {
                            { 1, ScopeName.ClassName },
                            { 2, ScopeName.Variable},
                        }),
                new LanguageRule(@"\b([A-Z][a-z0-9A-Z]*)\.([A-Z][a-z0-9A-Z]*)(\()",
                    new Dictionary<int, string>
                        {
                            { 1, ScopeName.ClassName },
                            { 2, ScopeName.Function},
                            { 3, ScopeName.ControlKeyword },
                        }),
                new LanguageRule(@"\b([a-z][a-z0-9A-Z]*)\.([A-Z][a-z0-9A-Z]*)(\()",
                    new Dictionary<int, string>
                        {
                            { 1, ScopeName.Variable },
                            { 2, ScopeName.Function },
                            { 3, ScopeName.ControlKeyword },
                        }),
                new LanguageRule(@"\.([A-Z][a-z0-9A-Z]*)(\()",
                    new Dictionary<int, string>
                        {
                            { 1, ScopeName.Function },
                            { 2, ScopeName.ControlKeyword },
                        }),
                new LanguageRule(
                    @"\b(int|string|float|double|bool|object|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\b",
                    new Dictionary<int, string>
                    {
                        { 1, ScopeName.Keyword },
                        { 2, ScopeName.Variable }
                    }),

                new LanguageRule(
                    @"\b([a-z0-9]+)\.\b",
                    new Dictionary<int, string>
                    {
                        { 1, ScopeName.Variable }
                    }),
                new LanguageRule(
                    @"\b([A-Z][a-zA-Z0-0]*)\b =",
                    new Dictionary<int, string>
                    {
                        { 1, ScopeName.Variable }
                    }),
                new LanguageRule(
                    @"\[(assembly|module|type|return|param|method|field|property|event):[^\]""]*(""[^\n]*?(?<!\\)"")?[^\]]*\]",
                    new Dictionary<int, string>
                        {
                            { 1, ScopeName.Keyword },
                            { 2, ScopeName.String }
                        }),
                new LanguageRule(
                    @"^\s*(\#define|\#elif|\#else|\#endif|\#endregion|\#error|\#if|\#line|\#pragma|\#region|\#undef|\#warning).*?$",
                    new Dictionary<int, string>
                        {
                            { 1, ScopeName.PreprocessorKeyword }
                        }),
                new LanguageRule(@"(new)\s+([\w.]+)(?:<[^>]*>)?(?:\(\))?",
                    new Dictionary<int, string>
                    {
                        { 1, ScopeName.Keyword },
                        { 2, ScopeName.ClassName },
                    }),
                new LanguageRule(@" +([A-Z][a-z0-9A-Z]*)[<\w+>]*(\()",
                    new Dictionary<int, string>
                    {
                        { 1, ScopeName.Function },
                        { 2, ScopeName.ControlKeyword },
                    }),
                new LanguageRule(
                    @"\b(abstract|as|ascending|base|bool|break|by|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|descending|do|double|dynamic|else|enum|equals|event|explicit|extern|false|finally|fixed|float|for|foreach|from|get|goto|group|if|implicit|in|int|into|interface|internal|is|join|let|lock|long|namespace|new|null|object|on|operator|orderby|out|override|params|partial|private|protected|public|readonly|ref|return|sbyte|sealed|select|set|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|var|virtual|void|volatile|where|while|yield|async|await|warning|disable)\b",
                    new Dictionary<int, string>
                        {
                            { 0, ScopeName.Keyword }
                        }),


                new LanguageRule(
                    @"(?s)(""[^\n]*?(?<!\\)"")",
                    new Dictionary<int, string>
                        {
                            { 0, ScopeName.String }
                        }),

                new LanguageRule(
                    @"\b[0-9]{1,}\b",
                    new Dictionary<int, string>
                        {
                            { 0, ScopeName.Number }
                        }),
            ];
        }
    }

    public bool HasAlias(string lang)
    {
        return lang.ToLower() switch
        {
            "cs" or "c#" or "csharp" or "cake" => true,
            _ => false,
        };
    }

    public override string ToString()
    {
        return Name;
    }
}