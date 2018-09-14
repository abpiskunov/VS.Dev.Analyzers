using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace VS.Dev.Analyzers
{
    /// <summary>
    /// Event though "" is optimized by compiler, for consistency it might be good 
    /// to use string.Empty everywhere (when possible).
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringEmptyLiteralAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "StringEmptyLiteral";

        private static readonly LocalizableString _title = 
            new LocalizableResourceString(
                nameof(Resources.StringEmptyLiteralAnalyzer_Title), 
                Resources.ResourceManager, 
                typeof(Resources));

        private static readonly LocalizableString _messageFormat = 
            new LocalizableResourceString(
                nameof(Resources.StringEmptyLiteralAnalyzer_MessageFormat), 
                Resources.ResourceManager, 
                typeof(Resources));

        private static readonly LocalizableString _description = 
            new LocalizableResourceString(
                nameof(Resources.StringEmptyLiteralAnalyzer_Description), 
                Resources.ResourceManager, 
                typeof(Resources));

        private static readonly DiagnosticDescriptor _rule = 
            new DiagnosticDescriptor(
                DiagnosticId, 
                _title, 
                _messageFormat, 
                Constants.Categories.Syntax, 
                DiagnosticSeverity.Info, 
                isEnabledByDefault: true, 
                description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.StringLiteralExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var expressionNode = (LiteralExpressionSyntax)context.Node;
            if (expressionNode == null ||
                expressionNode.Token.Kind() != SyntaxKind.StringLiteralToken||
               !string.IsNullOrEmpty(expressionNode.Token.ValueText))
            {
                return;
            }

            // check if node belongs to supported parent node
            var ancestorNode = expressionNode.FindParent(new[] { SyntaxKind.LocalDeclarationStatement, SyntaxKind.FieldDeclaration });
            if (ancestorNode == null)
            {
                return;
            }

            // check if parent does not have const declarator
            if (ancestorNode.ChildTokens().Any(x => x.Kind() == SyntaxKind.ConstKeyword))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(_rule, expressionNode.GetLocation(), expressionNode.Token.ValueText);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
