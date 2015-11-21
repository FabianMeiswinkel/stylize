// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using Stylize.Engine;
using Stylize.Engine.Rules;

namespace Stylize.Rules.CSharp
{
    [ExportStyleRule(Name, LanguageNames.CSharp)]
    [OrderStyleRule(Before = OrderedUsingsStyleRule.Name)]
    [OrderStyleRule(Before = UnusedUsingsStyleRule.Name)]
    [OrderStyleRule(Before = VSFormatStyleRule.Name)]
    public class OverQualifiedNameStyleRule : IStyleRule
    {
        public const string Name = "OverQualifiedNameStyleRule";

        public async Task<Solution> EnforceAsync(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            Document newDocument = await ImportAdder.AddImportsAsync(document);
            newDocument = await Simplifier.ReduceAsync(newDocument);

            // Occasionally the ImportAdder and/or the Simplifier will add unnecessary annotations to the document.
            // This results in no text changes, but in a different document.  So we'll use the presence of text changes
            // to determine if enforcement actually occurred;
            IEnumerable<TextChange> changes = await newDocument.GetTextChangesAsync(document);
            if (!changes.Any())
            {
                return document.Project.Solution;
            }

            // Roslyn's ImportAdder doesn't add trivia.  This is probably because it was intended to be followed by the
            // Formatter.  The net result is that if the formatter is not run after an import is added, the using
            // statement is malformed because it does not contain spaces.  While one of the principals in Stylize is
            // orthogonal rules, if the VSFormatStyleRule is not enabled, we will corrupt the document.  Consequently,
            // let's run a fix-up just for the UsingDirectiveSyntax nodes here.
            SyntaxNode root = await newDocument.GetSyntaxRootAsync();
            root = new UsingTriviaFixer(newDocument).Visit(root);
            newDocument = newDocument.WithSyntaxRoot(root);

            Log.WriteInformation("{0}: Simplified type declarations and member accesses", document.Name);

            return newDocument.Project.Solution;
        }

        class UsingTriviaFixer : CSharpSyntaxRewriter
        {
            readonly SyntaxTrivia newLineTrivia;

            public UsingTriviaFixer(Document document)
            {
                this.newLineTrivia = SyntaxFactory.EndOfLine(document.GetOption(GlobalOptions.NewLineText));
            }

            public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
            {
                if (node == null) { throw new ArgumentNullException(nameof(node)); }

                if (node.UsingKeyword.TrailingTrivia.FullSpan.IsEmpty)
                {
                    node = node.WithUsingKeyword(node.UsingKeyword.WithTrailingTrivia(SyntaxFactory.Space));
                }

                if (node.SemicolonToken.TrailingTrivia.FullSpan.IsEmpty)
                {
                    node = node.WithSemicolonToken(node.SemicolonToken.WithTrailingTrivia(this.newLineTrivia));
                }

                return node;
            }
        }
    }
}
