// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Stylize.Engine;
using Stylize.Engine.Rules;

namespace Stylize.Rules.CSharp
{
    [ExportStyleRule(Name, LanguageNames.CSharp)]
    [OrderStyleRule(Before = DuplicateNewLineStyleRule.Name)]
    public class VSFormatStyleRule : IStyleRule
    {
        public const string Name = "VSFormatStyleRule";

        public async Task<Solution> EnforceAsync(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            Document newDocument = await Formatter.FormatAsync(document);
            if (document != newDocument)
            {
                Log.WriteInformation("{0}: Applying VS whitespace formatting", document.Name);
                document = newDocument;
            }

            return document.Project.Solution;
        }
    }
}
