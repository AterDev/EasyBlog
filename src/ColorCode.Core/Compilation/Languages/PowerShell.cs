// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using ColorCode.Core;
using ColorCode.Core.Common;

namespace ColorCode.Core.Compilation.Languages;

public class PowerShell : ILanguage
{
    public string Id
    {
        get { return LanguageId.PowerShell; }
    }

    public string Name
    {
        get { return "PowerShell"; }
    }

    public string CssClassName
    {
        get { return "powershell"; }
    }

    public string FirstLinePattern
    {
        get { return null; }
    }

    public IList<LanguageRule> Rules
    {
        get
        {
            return
                       [
                           new LanguageRule(
                               @"(?s)(<\#.*?\#>)",
                               new Dictionary<int, string>
                                   {
                                       {1, ScopeName.Comment}
                                   }),
                           new LanguageRule(
                               @"(\#.*?)\r?$",
                               new Dictionary<int, string>
                                   {
                                       {1, ScopeName.Comment}
                                   }),
                           new LanguageRule(
                               @"'[^\n]*?(?<!\\)'",
                               new Dictionary<int, string>
                                   {
                                       {0, ScopeName.String}
                                   }),
                           new LanguageRule(
                               @"(?s)@"".*?""@",
                               new Dictionary<int, string>
                                   {
                                       {0, ScopeName.StringCSharpVerbatim}
                                   }),
                           new LanguageRule(
                               @"(?s)(""[^\n]*?(?<!`)"")",
                               new Dictionary<int, string>
                                   {
                                       {0, ScopeName.String}
                                   }),
                           new LanguageRule(
                               @"\$(?:[\d\w\-]+(?::[\d\w\-]+)?|\$|\?|\^)",
                               new Dictionary<int, string>
                                   {
                                       {0, ScopeName.PowerShellVariable}
                                   }),
                           new LanguageRule(
                               @"\${[^}]+}",
                               new Dictionary<int, string>
                                   {
                                       {0, ScopeName.PowerShellVariable}
                                   }),
                           new LanguageRule(
                               @"(?i)\b(begin|break|catch|continue|data|do|dynamicparam|elseif|else|end|exit|filter|finally|foreach|for|from|function|if|in|param|process|return|switch|throw|trap|try|until|while)\b",
                               new Dictionary<int, string>
                                   {
                                       {1, ScopeName.Keyword}
                                   }),
                           new LanguageRule(
                               @"\b(\w+\-\w+)\b",
                               new Dictionary<int, string>
                                   {
                                       {1, ScopeName.PowerShellCommand}
                                   }),
                           new LanguageRule(
                               @"-(?:c|i)?(?:eq|ne|gt|ge|lt|le|notlike|like|notmatch|match|notcontains|contains|replace)",
                               new Dictionary<int, string>
                                   {
                                       {0, ScopeName.PowerShellOperator}
                                   }
                               ),
                           new LanguageRule(
                               @"-(?:band|and|as|join|not|bxor|xor|bor|or|isnot|is|split)",
                               new Dictionary<int, string>
                                   {
                                       {0, ScopeName.PowerShellOperator}
                                   }
                               ),
                           new LanguageRule(
                               @"-\w+\d*\w*",
                               new Dictionary<int, string>
                                   {
                                       {0, ScopeName.PowerShellParameter}
                                   }
                               ),
                           new LanguageRule(
                               @"(?:\+=|-=|\*=|/=|%=|=|\+\+|--|\+|-|\*|/|%|\||,)",
                               new Dictionary<int, string>
                                   {
                                       {0, ScopeName.PowerShellOperator}
                                   }
                               ),
                           new LanguageRule(
                               @"(?:\>\>|2\>&1|\>|2\>\>|2\>)",
                               new Dictionary<int, string>
                                   {
                                       {0, ScopeName.PowerShellOperator}
                                   }
                               ),
                           new LanguageRule(
                               @"(?is)\[(cmdletbinding|alias|outputtype|parameter|validatenotnull|validatenotnullorempty|validatecount|validateset|allownull|allowemptycollection|allowemptystring|validatescript|validaterange|validatepattern|validatelength|supportswildcards)[^\]]+\]",
                               new Dictionary<int, string>
                                   {
                                       {1, ScopeName.PowerShellAttribute}
                                   }),
                           new LanguageRule(
                               @"(\[)([^\]]+)(\])(::)?",
                               new Dictionary<int, string>
                                   {
                                       {1, ScopeName.PowerShellOperator},
                                       {2, ScopeName.PowerShellType},
                                       {3, ScopeName.PowerShellOperator},
                                       {4, ScopeName.PowerShellOperator}
                                   })
                       ];
        }
    }

    public bool HasAlias(string lang)
    {
        return lang.ToLower() switch
        {
            "posh" or "ps1" or "pwsh" => true,
            _ => false,
        };
    }
}