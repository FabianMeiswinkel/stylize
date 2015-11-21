// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using Microsoft.CodeAnalysis;

namespace Stylize.Engine
{
    public static class SyntaxExtensions
    {
        public static bool IsEquivalentTo<TNode>(this SyntaxList<TNode> first, SyntaxList<TNode> second)
            where TNode : SyntaxNode
        {
            if (first.Count != second.Count)
            {
                return false;
            }

            for (int i = 0; i < first.Count; i++)
            {
                if (!first[i].IsEquivalentTo(second[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsEquivalentTo(this SyntaxTriviaList first, SyntaxTriviaList second)
        {
            if (first.Count != second.Count)
            {
                return false;
            }

            for (int i = 0; i < first.Count; i++)
            {
                if (!first[i].IsEquivalentTo(second[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
