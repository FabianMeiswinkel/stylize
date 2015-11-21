// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;

namespace Stylize.Engine.Matchers
{
    [ExportDocumentMatcher(Name)]
    public class AttributeDocumentMatcher : IDocumentMatcher
    {
        public const string Name = "AttributeDocumentMatcher";

        [ExportStylizeOption]
        public static Option<IReadOnlyList<string>> AttributeNames { get; } =
            new Option<IReadOnlyList<string>>(Name, "attributeNames", defaultValue: new string[0]);

        public async Task<bool> IsMatchAsync(Document document)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync();

            var matcher = new AttributeSyntaxMatcher(document.GetOption(AttributeNames), semanticModel);
            matcher.Visit(await document.GetSyntaxRootAsync());

            return matcher.HasMatch;
        }

        class AttributeSyntaxMatcher : SyntaxWalker
        {
            readonly IReadOnlyCollection<string> attributeNames;
            readonly SemanticModel semanticModel;

            public AttributeSyntaxMatcher(IReadOnlyCollection<string> attributeNames, SemanticModel semanticModel)
                : base(depth: SyntaxWalkerDepth.Node)
            {
                this.attributeNames = attributeNames;
                this.semanticModel = semanticModel;
            }

            public bool HasMatch { get; private set; }

            public override void Visit(SyntaxNode node)
            {
                // Once we have found a match, short-circuit all further searches
                if (this.HasMatch)
                {
                    return;
                }

                ISymbol declaredSymbol = this.semanticModel.GetDeclaredSymbol(node);
                if (declaredSymbol != null)
                {
                    ImmutableArray<AttributeData> attributes = declaredSymbol.GetAttributes();
                    if (attributes.Any(a =>
                        this.attributeNames.Contains(a.AttributeClass.Name) ||
                        this.attributeNames.Contains(a.AttributeClass.ToString())))
                    {
                        this.HasMatch = true;
                        return;
                    }
                }

                base.Visit(node);
            }
        }
    }
}
