// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Stylize.Engine;
using Stylize.Engine.Rules;

namespace Stylize.Rules.CSharp
{
    [ExportStyleRule(Name, LanguageNames.CSharp)]
    public class OrderedUsingsStyleRule : IStyleRule
    {
        public const string Name = "OrderedUsingsStyleRule";

        [ExportStylizeOption]
        public static Option<bool> SystemUsingsFirst { get; } = new Option<bool>(Name, "systemUsingsFirst");

        public async Task<Solution> EnforceAsync(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync();

            var syntaxRewriter = new OrderedUsingsSyntaxRewriter(document.GetOption(SystemUsingsFirst));
            SyntaxNode newRoot = syntaxRewriter.Visit(oldRoot);

            if (oldRoot != newRoot)
            {
                Log.WriteInformation("{0}: Ordering usings and aliases", document.Name);
                document = document.WithSyntaxRoot(newRoot);
            }

            return document.Project.Solution;
        }

        class OrderedUsingsSyntaxRewriter : CSharpSyntaxRewriter
        {
            readonly UsingDirectiveSyntaxComparer comparer;

            public OrderedUsingsSyntaxRewriter(bool systemUsingsFirst)
            {
                this.comparer = new UsingDirectiveSyntaxComparer(systemUsingsFirst);
            }

            SyntaxList<UsingDirectiveSyntax> SortUsings(SyntaxList<UsingDirectiveSyntax> usings)
            {
                if (usings.Count == 0)
                {
                    return usings;
                }

                IEnumerable<UsingDirectiveSyntax> orderedUsings = usings.OrderBy(u => u, this.comparer);
                var sortedUsings = new SyntaxList<UsingDirectiveSyntax>().AddRange(orderedUsings);

                if (sortedUsings.IsEquivalentTo(usings))
                {
                    // If the sort has not re-ordered the usings, then just return the original list
                    return usings;
                }

                // If the first using has changed position, then copy the leading trivia (e.g., header comment) to the
                // new first using.
                if (!sortedUsings[0].IsEquivalentTo(usings[0]))
                {
                    UsingDirectiveSyntax oldFirstUsing = sortedUsings.First(u => u.IsEquivalentTo(usings[0]));
                    SyntaxTriviaList leadingTrivia = oldFirstUsing.GetLeadingTrivia();
                    sortedUsings = sortedUsings.Replace(oldFirstUsing, oldFirstUsing.WithoutLeadingTrivia());
                    sortedUsings = sortedUsings.Replace(
                        sortedUsings[0], sortedUsings[0].WithLeadingTrivia(leadingTrivia));
                }

                return sortedUsings;
            }

            public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
            {
                if (node == null) { throw new ArgumentNullException(nameof(node)); }

                node = node.WithUsings(this.SortUsings(node.Usings));
                return base.VisitCompilationUnit(node);
            }

            public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                if (node == null) { throw new ArgumentNullException(nameof(node)); }

                node = node.WithUsings(this.SortUsings(node.Usings));
                return base.VisitNamespaceDeclaration(node);
            }
        }

        class UsingDirectiveSyntaxComparer : IComparer<UsingDirectiveSyntax>
        {
            static readonly StringComparer NameComparer = StringComparer.Ordinal;
            const string SystemNamespace = "System";

            readonly bool systemUsingsFirst;

            public UsingDirectiveSyntaxComparer(bool systemUsingsFirst)
            {
                this.systemUsingsFirst = systemUsingsFirst;
            }

            static Stack<SimpleNameSyntax> ExtractNameComponents(NameSyntax name)
            {
                var nameComponents = new Stack<SimpleNameSyntax>();

                while (true)
                {
                    var simpleName = name as SimpleNameSyntax;
                    if (simpleName != null)
                    {
                        nameComponents.Push(simpleName);
                        break;
                    }

                    var qualifiedName = name as QualifiedNameSyntax;
                    if (qualifiedName != null)
                    {
                        nameComponents.Push(qualifiedName.Right);
                        name = qualifiedName.Left;
                        continue;
                    }

                    throw new ArgumentException(
                        $"Unexpected NameSyntax in using directive of kind {name.Kind()}", nameof(name));
                }

                return nameComponents;
            }

            public int Compare(UsingDirectiveSyntax x, UsingDirectiveSyntax y)
            {
                if (x == null)
                {
                    return y == null ? 0 : -1;
                }

                if (y == null)
                {
                    return 1;
                }

                // Aliases are at the bottom, sorted by alias identifier
                if (x.Alias != null || y.Alias != null)
                {
                    // Null is treated as less than any other value.  Consequently, if only one of {x, y} is an alias,
                    // then the non-alias identifier will be null, and it will be ordered before the alias.  If both
                    // are aliases, then the identifier will be used to sort them.
                    return StringComparer.Ordinal.Compare(
                        x.Alias?.Name.Identifier.ValueText,
                        y.Alias?.Name.Identifier.ValueText);
                }

                // In the middle are static usings, sorted by namespace identifier
                bool isXStatic = x.StaticKeyword.IsKind(SyntaxKind.StaticKeyword);
                bool isYStatic = y.StaticKeyword.IsKind(SyntaxKind.StaticKeyword);
                int staticComparison = isXStatic.CompareTo(isYStatic);
                if (staticComparison != 0)
                {
                    // Only one of the two usings is static - it comes second.
                    return staticComparison;
                }

                // At this point we are either dealing with two static usings or two namespace usings.  Either way we
                // will compare the name components (i.e., qualified identifiers).
                Stack<SimpleNameSyntax> xNameComponents = ExtractNameComponents(x.Name);
                Stack<SimpleNameSyntax> yNameComponents = ExtractNameComponents(y.Name);

                if (this.systemUsingsFirst)
                {
                    bool xIsSystem = NameComparer.Equals(xNameComponents.Peek().ToString(), SystemNamespace);
                    bool yIsSystem = NameComparer.Equals(yNameComponents.Peek().ToString(), SystemNamespace);

                    int systemComparison = xIsSystem.CompareTo(yIsSystem);
                    if (systemComparison != 0)
                    {
                        // Only one of the two usings is in the System namespace - it comes first (hence the negation).
                        return -systemComparison;
                    }

                    // Either both usings are in the System namespace or both are not - the below general case
                    // will compare the name components.
                }

                while (xNameComponents.Count > 0 && yNameComponents.Count > 0)
                {
                    string xName = xNameComponents.Pop().ToString();
                    string yName = yNameComponents.Pop().ToString();

                    int nameComparison = NameComparer.Compare(xName, yName);
                    if (nameComparison != 0)
                    {
                        return nameComparison;
                    }
                }

                // All components matched so far, but one or both identifiers are now out of components.  The shorter
                // of the two will come first.
                return xNameComponents.Count.CompareTo(yNameComponents.Count);
            }
        }
    }
}
