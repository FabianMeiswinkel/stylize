// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Stylize.Rules.CSharp
{
    static class SyntaxNodeExtensions
    {
        public static bool HasAncestorKind(this SyntaxNode node, params SyntaxKind[] kinds)
        {
            return node.Ancestors().Any(n => kinds.Contains(n.Kind()));
        }
    }
}
