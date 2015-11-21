// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Options;
using Stylize.Engine;
using Stylize.Engine.Rules;

namespace Stylize.Rules.CSharp
{
    /// <summary>
    /// Adds (or optionally removes) header comments from source files.
    /// </summary>
    [ExportStyleRule(Name, LanguageNames.CSharp)]
    public class HeaderCommentStyleRule : IStyleRule
    {
        const string CommentPrefix = "// ";
        public const string Name = "HeaderCommentStyleRule";

        readonly Dictionary<IReadOnlyList<string>, SyntaxTriviaList> commentCache =
            new Dictionary<IReadOnlyList<string>, SyntaxTriviaList>();

        [ExportStylizeOption]
        public static Option<IReadOnlyList<string>> HeaderComments { get; } =
            new Option<IReadOnlyList<string>>(Name, "headerComments", defaultValue: new string[0]);

        static SyntaxTriviaList BuildCommentTrivia(IEnumerable<string> headerComments, string newLineText)
        {
            SyntaxTrivia newLineTrivia = SyntaxFactory.EndOfLine(newLineText);
            var commentTrivia = new SyntaxTriviaList();

            var hasHeaderComments = false;
            foreach (string headerComment in headerComments)
            {
                hasHeaderComments = true;

                commentTrivia = commentTrivia.Add(SyntaxFactory.Comment(CommentPrefix + headerComment));
                commentTrivia = commentTrivia.Add(newLineTrivia);
            }

            if (hasHeaderComments)
            {
                // Add an extra empty line below the header comments, if present.
                commentTrivia = commentTrivia.Add(newLineTrivia);
            }

            return commentTrivia;
        }

        public async Task<Solution> EnforceAsync(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            IReadOnlyList<string> headerComments = document.GetOption(HeaderComments);
            string newLineText = document.GetOption(GlobalOptions.NewLineText);
            SyntaxTriviaList commentTrivia = this.commentCache.GetOrAdd(
                headerComments, c => BuildCommentTrivia(headerComments, newLineText));

            SyntaxNode syntaxRoot = await document.GetSyntaxRootAsync();

            if (syntaxRoot.HasLeadingTrivia)
            {
                SyntaxTriviaList leadingTrivia = syntaxRoot.GetLeadingTrivia();
                if (!leadingTrivia.IsEquivalentTo(commentTrivia))
                {
                    Log.WriteInformation("{0}: Rewriting non-conforming header comment", document.Name);
                    syntaxRoot = syntaxRoot.WithLeadingTrivia().WithLeadingTrivia(commentTrivia);
                    document = document.WithSyntaxRoot(syntaxRoot);
                }
            }
            else if (commentTrivia.Count > 0)
            {
                Log.WriteInformation("{0}: Adding missing header comment", document.Name);
                syntaxRoot = syntaxRoot.WithLeadingTrivia(commentTrivia);
                document = document.WithSyntaxRoot(syntaxRoot);
            }

            return document.Project.Solution;
        }
    }
}
