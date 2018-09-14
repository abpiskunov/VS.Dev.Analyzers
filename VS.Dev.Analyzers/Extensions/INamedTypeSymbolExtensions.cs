using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace VS.Dev.Analyzers
{
    internal static class INamedTypeSymbolExtensions
    {
        public static INamedTypeSymbol FindParent(this INamedTypeSymbol nodeSymbol, IEnumerable<string> parentFullTypes)
        {
            nodeSymbol = nodeSymbol?.BaseType;
            while (nodeSymbol != null)
            {
                var nodeFullName = nodeSymbol.ToString();
                if (parentFullTypes.Any(x => nodeFullName.Equals(x, StringComparison.Ordinal)))
                {
                    break;
                }

                nodeSymbol = nodeSymbol.BaseType;
            }

            return nodeSymbol;
        }
    }
}
