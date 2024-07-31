// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using ColorCode.Core;
using ColorCode.Core.Common;

namespace ColorCode.Core.Compilation.Languages;

public class MatLab : ILanguage
{
    public string Id
    {
        get { return LanguageId.MatLab; }
    }

    public string Name
    {
        get { return "MATLAB"; }
    }

    public string CssClassName
    {
        get { return "matlab"; }
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
            return
                       [                                       
                           // regular comments
                           new LanguageRule(
                               @"(%.*)\r?",
                               new Dictionary<int, string>
                                   {
                                       { 0, ScopeName.Comment },
                                   }),
                                   
                           // regular strings
                           new LanguageRule(
                               @"(?<!\.)('[^\n]*?')",
                               new Dictionary<int, string>
                                   {
                                       { 0, ScopeName.String },
                                   }),
                           new LanguageRule(
                               @"""[^\n]*?""",
                               new Dictionary<int, string>
                                   {
                                       { 0, ScopeName.String },
                                   }),
                           
                           // keywords
                           new LanguageRule(
                               @"(?i)\b(break|case|catch|continue|else|elseif|end|for|function|global|if|otherwise|persistent|return|switch|try|while)\b",
                               new Dictionary<int, string>
                                   {
                                       { 1, ScopeName.Keyword },
                                   }),
                                   
                           // line continuation
                            new LanguageRule(
                               @"\.\.\.",
                               new Dictionary<int, string>
                                   {
                                       { 0, ScopeName.Continuation },
                                   }),
                                   
                            // numbers
                            new LanguageRule(
                               @"\b([0-9.]|[0-9.]+(e-*)(?=[0-9]))+?\b",
                               new Dictionary<int, string>
                                   {
                                       { 0, ScopeName.Number },
                                   }),
                       ];
        }
    }

    public bool HasAlias(string lang)
    {
        return lang.ToLower() switch
        {
            "m" => true,
            "mat" => true,
            "matlab" => true,
            _ => false,
        };
    }

    public override string ToString()
    {
        return Name;
    }
}