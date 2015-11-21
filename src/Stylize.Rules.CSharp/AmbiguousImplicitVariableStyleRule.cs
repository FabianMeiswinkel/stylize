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
    [OrderStyleRule(Before = OverQualifiedNameStyleRule.Name)]
    [OrderStyleRule(Before = VSFormatStyleRule.Name)]
    public class AmbiguousImplicitVariableStyleRule : IStyleRule
    {
        public const string Name = "AmbiguousImplicitVariableStyleRule";

        [ExportStylizeOption]
        public static Option<bool> EnforceForEachStatements { get; } = new Option<bool>(Name, "enforceForEachStatements");

        public async Task<Solution> EnforceAsync(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            SemanticModel semanticModel = await document.GetSemanticModelAsync();
            var syntaxRewriter = new ImplicitVariableSyntaxRewriter(
                document.GetOption(EnforceForEachStatements), semanticModel);

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync();
            SyntaxNode newRoot = syntaxRewriter.Visit(oldRoot);

            if (oldRoot != newRoot)
            {
                Log.WriteInformation("{0}: Correcting implicit variable declarations", document.Name);
                document = document.WithSyntaxRoot(newRoot);
            }

            return document.Project.Solution;
        }

        class ImplicitVariableSyntaxRewriter : CSharpSyntaxRewriter
        {
            readonly bool enforceForEachStatements;
            readonly SemanticModel semanticModel;
            readonly INamedTypeSymbol unboundIEnumerableSymbol;

            public ImplicitVariableSyntaxRewriter(bool enforceForEachStatements, SemanticModel semanticModel)
            {
                this.enforceForEachStatements = enforceForEachStatements;
                this.semanticModel = semanticModel;
                this.unboundIEnumerableSymbol = this.semanticModel.Compilation.GetTypeByMetadataName(
                    typeof(IEnumerable<>).FullName);
            }

            TypeSyntax CorrectType(TypeSyntax type, SyntaxNode initializer, ITypeSymbol initializerSymbol)
            {
                ITypeSymbol typeSymbol = this.semanticModel.GetTypeInfo(type).Type;
                if (typeSymbol == null)
                {
                    Log.WriteWarning("Unable to determine symbol for type {0}", type);
                    return type;
                }

                // If the semantic model was unable to determine the symbol of the type, we don't have the context to
                // make the right decision here, so let's skip enforcement for this declaration.
                if (typeSymbol.SymbolOrSubSymbols(s => s.Kind == SymbolKind.ErrorType))
                {
                    return type;
                }

                // If it is an anonymous type, then let's just move on!
                if (typeSymbol.SymbolOrSubSymbols(s => s.IsAnonymousType))
                {
                    return type;
                }

                bool isAmbiguous = this.IsAmbiguous(typeSymbol, initializer, initializerSymbol);

                // Ambiguous var - change to explicit type
                if (type.IsVar && isAmbiguous)
                {
                    return SyntaxFactory.IdentifierName(typeSymbol.ToString()).WithTriviaFrom(type);
                }

                // Unambiguous explicit type - change to var
                if (!type.IsVar && !isAmbiguous)
                {
                    return SyntaxFactory.IdentifierName("var").WithTriviaFrom(type);
                }

                return type;
            }

            bool IsAmbiguous(ISymbol typeSymbol, SyntaxNode initializer, ITypeSymbol initializerSymbol)
            {
                if (!typeSymbol.Equals(initializerSymbol))
                {
                    // An implicit conversion or something similar is at play - using an implicit declaration will
                    // have a semantic change.  Consequently, we consider the declaration ambiguous.
                    return true;
                }

                if (initializer is LiteralExpressionSyntax)
                {
                    // A literal initializer is not ambiguous per C# style guidelines.
                    return false;
                }

                var ambiguityDetector = new InitializerAmbiguityDetector(this.semanticModel, typeSymbol);
                ambiguityDetector.Visit(initializer);
                return ambiguityDetector.IsAmbiguous;
            }

            public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
            {
                if (node == null) { throw new ArgumentNullException(nameof(node)); }

                if (!this.enforceForEachStatements)
                {
                    return base.VisitForEachStatement(node);
                }

                // We can only access the semantic model for the original SyntaxNode tree.  Consequently, we need to
                // visit (and potentially rebuild) the inner foreach statement block before we potentially rewrite
                // the local type (and consequently change this and descendant nodes).
                var visitedNode = (ForEachStatementSyntax)base.VisitForEachStatement(node);

                // The "initializer" symbol for foreach statements is considered to be the type argument from the
                // IEnumerable<> of the expression.
                ITypeSymbol expressionSymbol = this.semanticModel.GetTypeInfo(node.Expression).Type;

                // First check if the expression symbol itself is IEnumerable<>
                var iEnumerableSymbol = expressionSymbol as INamedTypeSymbol;
                if (iEnumerableSymbol == null ||
                    !this.unboundIEnumerableSymbol.Equals(iEnumerableSymbol.ConstructedFrom))
                {
                    // Not the case... Does the symbol at least implement IEnumerable<>?
                    iEnumerableSymbol = expressionSymbol.AllInterfaces.FirstOrDefault(
                        i => i.ConstructedFrom.Equals(this.unboundIEnumerableSymbol));
                    if (iEnumerableSymbol == null)
                    {
                        // The type seems to only be an IEnumerable (not generic).  Let's not correct the declaration
                        // because we might be missing some context here.
                        return visitedNode;
                    }
                }

                return visitedNode.WithType(
                    this.CorrectType(node.Type, node.Expression, iEnumerableSymbol.TypeArguments[0]));
            }

            public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
            {
                if (node == null) { throw new ArgumentNullException(nameof(node)); }

                // Const declarations cannot use implicit variables, so leave it alone.
                if (node.IsConst)
                {
                    return node;
                }

                VariableDeclarationSyntax declaration = node.Declaration;

                // Implicitly typed variables must each be declared separately.  For now, we will not handle this case.
                // TODO: Investigate splitting variable declarations
                if (declaration.Variables.Count != 1)
                {
                    return node;
                }

                // If it is a declaration without initialization, there is nothing to do here.
                if (declaration.Variables[0].Initializer == null)
                {
                    return node;
                }

                ExpressionSyntax initializer = declaration.Variables[0].Initializer.Value;
                ITypeSymbol initializerSymbol = this.semanticModel.GetTypeInfo(initializer).Type;
                declaration = declaration.WithType(this.CorrectType(declaration.Type, initializer, initializerSymbol));

                return node.WithDeclaration(declaration);
            }

            public override SyntaxNode VisitUsingStatement(UsingStatementSyntax node)
            {
                if (node == null) { throw new ArgumentNullException(nameof(node)); }

                // We can only access the semantic model for the original SyntaxNode tree.  Consequently, we need to
                // visit (and potentially rebuild) the inner using statement block before we potentially rewrite
                // the declaration type (and consequently change this and descendant nodes).
                var visitedNode = (UsingStatementSyntax)base.VisitUsingStatement(node);

                // The using only contains an expression or an identifier, but no declaration.
                VariableDeclarationSyntax declaration = node.Declaration;
                if (declaration == null)
                {
                    return visitedNode;
                }

                ExpressionSyntax initializer = declaration.Variables[0].Initializer.Value;
                ITypeSymbol initializerSymbol = this.semanticModel.GetTypeInfo(initializer).Type;
                declaration = declaration.WithType(this.CorrectType(declaration.Type, initializer, initializerSymbol));

                return visitedNode.WithDeclaration(declaration);
            }
        }

        class InitializerAmbiguityDetector : CSharpSyntaxWalker
        {
            readonly SemanticModel semanticModel;
            readonly ISymbol typeSymbol;

            public InitializerAmbiguityDetector(SemanticModel semanticModel, ISymbol typeSymbol)
            {
                this.semanticModel = semanticModel;
                this.typeSymbol = typeSymbol;
            }

            public bool IsAmbiguous { get; private set; } = true;

            public override void DefaultVisit(SyntaxNode node)
            {
                if (this.typeSymbol.Equals(this.semanticModel.GetSymbolInfo(node).Symbol))
                {
                    this.IsAmbiguous = false;
                }
                else
                {
                    base.DefaultVisit(node);
                }
            }

            public override void VisitArgument(ArgumentSyntax node)
            {
                // Ignore arguments for the purposes of ambiguity detection (e.g., GetValue(Int32.MinValue) is
                // ambiguous even if typeSymbol is Int32).
            }

            public override void VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
            {
                if (!node.IsKind(SyntaxKind.ImplicitArrayCreationExpression) &&
                    this.typeSymbol.Equals(this.semanticModel.GetTypeInfo(node).Type))
                {
                    this.IsAmbiguous = false;
                }
                else
                {
                    base.DefaultVisit(node);
                }
            }

            public override void VisitNullableType(NullableTypeSyntax node)
            {
                if (this.typeSymbol.Equals(this.semanticModel.GetTypeInfo(node).Type))
                {
                    this.IsAmbiguous = false;
                }
            }
        }
    }
}
