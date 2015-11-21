// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Stylize.Engine;
using Stylize.Engine.Rules;

namespace Stylize.Rules.CSharp
{
    [ExportStyleRule(Name, LanguageNames.CSharp)]
    [OrderStyleRule(Before = OrderedUsingsStyleRule.Name)]
    public class UnusedUsingsStyleRule : IStyleRule
    {
        public const string Name = "UnusedUsingsStyleRule";

        public async Task<Solution> EnforceAsync(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            // The compiler will actually do the heavy lifting for us on this one: when it generates the semantic model
            // it creates diagnostic notes for usings that are unused with the id "CS8019".
            SemanticModel semanticModel = await document.GetSemanticModelAsync();
            IEnumerable<Diagnostic> diagnostics = semanticModel.GetDiagnostics().Where(d => d.Id == "CS8019");

            // Save the leading trivia to reattach after we have removed the unused roots
            SyntaxNode oldRoot = await document.GetSyntaxRootAsync();
            SyntaxTriviaList leadingTrivia = oldRoot.GetLeadingTrivia();

            // Now we need to go through the diagnostics in reverse order (so we don't corrupt our spans), find the
            // relevant SyntaxNodes, and remove them.
            diagnostics = diagnostics.OrderByDescending(d => d.Location.SourceSpan.Start);
            SyntaxNode newRoot = oldRoot;

            foreach (Diagnostic diagnostic in diagnostics)
            {
                newRoot = newRoot.RemoveNodes(
                    newRoot.DescendantNodes(diagnostic.Location.SourceSpan),
                    SyntaxRemoveOptions.KeepNoTrivia);
            }

            if (newRoot != oldRoot)
            {
                Log.WriteInformation("{0}: Removing unused usings", document.Name);
                document = document.WithSyntaxRoot(newRoot.WithLeadingTrivia(leadingTrivia));
            }

            return document.Project.Solution;
        }
    }
}
