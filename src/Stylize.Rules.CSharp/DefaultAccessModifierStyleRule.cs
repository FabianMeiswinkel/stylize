// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
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
    [OrderStyleRule(Before = VSFormatStyleRule.Name)]
    public class DefaultAccessModifierStyleRule : IStyleRule
    {
        public const string Name = "DefaultAccessModifierStyleRule";

        [ExportStylizeOption]
        public static Option<bool> ExplicitAccessModifier { get; } = new Option<bool>(Name, "explicitAccessModifier");

        public async Task<Solution> EnforceAsync(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync();

            bool explicitAccessModifier = document.GetOption(ExplicitAccessModifier);
            var syntaxRewriter = new AccessModifierSyntaxRewriter(explicitAccessModifier);
            SyntaxNode newRoot = syntaxRewriter.Visit(oldRoot);

            if (oldRoot != newRoot)
            {
                Log.WriteInformation(
                    "{0}: {1} default access modifiers",
                    document.Name,
                    explicitAccessModifier ? "Adding" : "Removing");
                document = document.WithSyntaxRoot(newRoot);
            }

            return document.Project.Solution;
        }

        class AccessModifierSyntaxRewriter : CSharpSyntaxRewriter
        {
            static readonly SyntaxKind[] AccessModifierKinds = new[]
            {
                SyntaxKind.InternalKeyword,
                SyntaxKind.PrivateKeyword,
                SyntaxKind.ProtectedKeyword,
                SyntaxKind.PublicKeyword
            };

            readonly bool explicitAccessModifiers;

            public AccessModifierSyntaxRewriter(bool explicitAccessModifiers)
            {
                this.explicitAccessModifiers = explicitAccessModifiers;
            }

            TNode UpdateAccessModifier<TNode>(
                TNode node,
                Func<TNode, SyntaxTokenList> getModifiers,
                Func<TNode, SyntaxTokenList, TNode> setModifiers,
                SyntaxKind defaultAccessModifier)
                where TNode : SyntaxNode
            {
                TNode trimmedNode = node.WithoutLeadingTrivia();
                SyntaxTokenList originalModifiers = getModifiers(trimmedNode);
                SyntaxTokenList updatedModifiers = originalModifiers;
                if (!this.explicitAccessModifiers)
                {
                    updatedModifiers = originalModifiers.Remove(
                        originalModifiers.FirstOrDefault(m => m.Kind() == defaultAccessModifier));
                }
                else if (!originalModifiers.Any(m => AccessModifierKinds.Contains(m.Kind())))
                {
                    updatedModifiers = originalModifiers.Insert(
                        index: 0,
                        token: SyntaxFactory.Token(defaultAccessModifier).WithTrailingTrivia(SyntaxFactory.Space));
                }

                if (originalModifiers == updatedModifiers)
                {
                    return node;
                }

                return setModifiers(trimmedNode, updatedModifiers).WithLeadingTrivia(node.GetLeadingTrivia());
            }

            static SyntaxKind GetNestableAccessModifier(SyntaxNode node)
            {
                if (node.HasAncestorKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration))
                {
                    return SyntaxKind.PrivateKeyword;
                }

                return SyntaxKind.InternalKeyword;
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    GetNestableAccessModifier(node));

                return base.VisitClassDeclaration(node);
            }

            public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    SyntaxKind.PrivateKeyword);

                return node;
            }

            public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    GetNestableAccessModifier(node));

                return node;
            }

            public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    SyntaxKind.PrivateKeyword);

                return node;
            }

            public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    GetNestableAccessModifier(node));

                return base.VisitStructDeclaration(node);
            }

            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    SyntaxKind.PrivateKeyword);

                return node;
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    SyntaxKind.PrivateKeyword);

                return node;
            }

            public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    SyntaxKind.PrivateKeyword);

                return node;
            }

            public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    GetNestableAccessModifier(node));

                return node;
            }

            public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    GetNestableAccessModifier(node));

                return node;
            }

            public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    SyntaxKind.PrivateKeyword);

                return node;
            }

            public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    SyntaxKind.PrivateKeyword);

                return node;
            }

            public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    SyntaxKind.PrivateKeyword);

                return node;
            }

            public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
            {
                node = this.UpdateAccessModifier(
                    node,
                    n => n.Modifiers,
                    (n, m) => n.WithModifiers(m),
                    SyntaxKind.PrivateKeyword);

                return node;
            }
        }
    }
}
