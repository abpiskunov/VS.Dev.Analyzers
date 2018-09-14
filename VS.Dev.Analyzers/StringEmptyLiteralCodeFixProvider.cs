using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VS.Dev.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StringEmptyLiteralCodeFixProvider)), Shared]
    public class StringEmptyLiteralCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get => ImmutableArray.Create(StringEmptyLiteralAnalyzer.DiagnosticId);
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var stringLiteralNode = (LiteralExpressionSyntax)root.FindToken(diagnosticSpan.Start).Parent;

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Resources.StringEmptyLiteralAnalyzer_MessageFormat,
                    createChangedDocument: c => ReplaceStringLiteralAsync(context.Document, stringLiteralNode, c),
                    equivalenceKey: Resources.StringEmptyLiteralAnalyzer_MessageFormat),
                diagnostic);
        }

        private async Task<Document> ReplaceStringLiteralAsync(
            Document document,
            LiteralExpressionSyntax stringLiteralNode, 
            CancellationToken cancellationToken)
        {
            var stringEmptyNode = SyntaxFactory.MemberAccessExpression(
                                   SyntaxKind.SimpleMemberAccessExpression,
                                   SyntaxFactory.IdentifierName(@"string"),
                                   SyntaxFactory.IdentifierName(@"Empty"))
                                 .WithOperatorToken(SyntaxFactory.Token(SyntaxKind.DotToken));

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(stringLiteralNode, stringEmptyNode);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
