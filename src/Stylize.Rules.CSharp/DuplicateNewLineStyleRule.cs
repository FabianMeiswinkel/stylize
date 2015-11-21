// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Stylize.Engine;
using Stylize.Engine.Rules;

namespace Stylize.Rules.CSharp
{
    [ExportStyleRule(Name, LanguageNames.CSharp)]
    public class DuplicateNewLineStyleRule : IStyleRule
    {
        public const string Name = "DuplicateNewLineStyleRule";

        readonly NewLineTriviaRewriter syntaxRewriter = new NewLineTriviaRewriter();

        public async Task<Solution> EnforceAsync(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync();
            SyntaxNode newRoot = this.syntaxRewriter.Visit(oldRoot);

            if (oldRoot != newRoot)
            {
                Log.WriteInformation("{0}: Removing duplicate new lines", document.Name);
                document = document.WithSyntaxRoot(newRoot);
            }

            return document.Project.Solution;
        }

        class NewLineTriviaRewriter : CSharpSyntaxRewriter
        {
            static SyntaxTriviaList FlushDuplicateTrivia(
                SyntaxTriviaList trivia, int startIndex, int endIndex, bool allowExtraNewLine)
            {
                int allowedNewLines = allowExtraNewLine ? 2 : 1;
                var newLineCount = 0;
                for (; endIndex >= startIndex; endIndex--)
                {
                    if (newLineCount == allowedNewLines)
                    {
                        trivia = trivia.RemoveAt(endIndex);
                    }
                    else if (trivia[endIndex].IsKind(SyntaxKind.EndOfLineTrivia))
                    {
                        newLineCount++;
                    }
                }

                return trivia;
            }

            static SyntaxTriviaList Clean(SyntaxTriviaList trivia)
            {
                var flushStartIndex = 0;
                var allowExtraNewLine = false;
                for (int i = 0; i < trivia.Count; i++)
                {
                    SyntaxTrivia currentTrivia = trivia[i];
                    if (!currentTrivia.IsKind(SyntaxKind.WhitespaceTrivia) &&
                        !currentTrivia.IsKind(SyntaxKind.EndOfLineTrivia))
                    {
                        // Found a non-spacing trivia - time to flush the pending trivia
                        SyntaxTriviaList newTrivia = FlushDuplicateTrivia(
                            trivia, flushStartIndex, i, allowExtraNewLine);

                        // Correct the loop index so when we finish our flush, we will resume the iteration at the next
                        // unprocessed trivia.
                        i -= trivia.Count - newTrivia.Count;
                        trivia = newTrivia;

                        flushStartIndex = i + 1;

                        // If the non-spacing trivia does not contain a new line in trailing structured sub-trivia,
                        // then allow an extra new line (this allows for scenarios like a comment followed by a blank
                        // line).
                        allowExtraNewLine = !currentTrivia.HasStructure ||
                            !currentTrivia.GetStructure().GetTrailingTrivia().Any(SyntaxKind.EndOfLineTrivia);
                    }
                }

                // Perform a final flush
                if (flushStartIndex < trivia.Count)
                {
                    trivia = FlushDuplicateTrivia(trivia, flushStartIndex, trivia.Count - 1, allowExtraNewLine);
                }

                return trivia;
            }

            public override SyntaxToken VisitToken(SyntaxToken token)
            {
                SyntaxTriviaList oldTrivia = token.LeadingTrivia;
                SyntaxTriviaList newTrivia = Clean(oldTrivia);

                if (oldTrivia != newTrivia)
                {
                    token = token.WithLeadingTrivia(newTrivia);
                }

                return token;
            }
        }
    }
}
