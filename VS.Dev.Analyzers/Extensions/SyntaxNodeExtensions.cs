using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace VS.Dev.Analyzers
{
    internal static class SyntaxNodeExtensions
    {
        public static SyntaxNode FindParent(this SyntaxNode node, IEnumerable<SyntaxKind> parentKinds)
        {
            node = node.Parent;
            while (node != null)
            {
                if (parentKinds.Any(x => node.IsKind(x)))
                {
                    break;
                }

                node = node.Parent;
            }

            return node;
        }
    }
}
