// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Stylize.Engine
{
    public static class TypeSymbolExtensions
    {
        public static bool SymbolOrSubSymbols(this ITypeSymbol symbol, Predicate<ITypeSymbol> predicate)
        {
            if (predicate == null) { throw new ArgumentNullException(nameof(predicate)); }

            if (predicate(symbol))
            {
                return true;
            }

            var namedSymbol = symbol as INamedTypeSymbol;
            if (namedSymbol != null)
            {
                return namedSymbol.TypeArguments.Any(s => predicate(s));
            }

            var arraySymbol = symbol as IArrayTypeSymbol;
            if (arraySymbol != null)
            {
                return predicate(arraySymbol.ElementType);
            }

            return false;
        }
    }
}
