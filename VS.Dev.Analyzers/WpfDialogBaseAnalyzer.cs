using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace VS.Dev.Analyzers
{
    /// <summary>
    /// If code behind class is based on standard System.Windows.Window this is wrong and may 
    /// lead to theming or accessibility bugs.
    /// 
    /// Wrong:   partial class X : System.Windows.Window
    /// Correct: partial class X : Microsoft.VisualStudio.PlatformUI.DialogWindow
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public partial class WpfDialogBaseAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WpfDialogBase";
        private static readonly string[] _suggestedBaseClass = new[] { "Microsoft.VisualStudio.PlatformUI.DialogWindow" };
        private static readonly string[] _unexpectedBaseClass = new[] { "System.Windows.Window" };

        private static readonly LocalizableString _title = 
            new LocalizableResourceString(
                nameof(Resources.WpfDialogBaseAnalyzerAnalyzer_Title), 
                Resources.ResourceManager, 
                typeof(Resources));

        private static readonly LocalizableString _messageFormat = 
            new LocalizableResourceString(
                nameof(Resources.WpfDialogBaseAnalyzerAnalyzer_MessageFormat), 
                Resources.ResourceManager, 
                typeof(Resources));

        private static readonly LocalizableString _description = 
            new LocalizableResourceString(
                nameof(Resources.WpfDialogBaseAnalyzerAnalyzer_Description), 
                Resources.ResourceManager, 
                typeof(Resources));

        private static readonly DiagnosticDescriptor _rule = 
            new DiagnosticDescriptor(
                DiagnosticId, 
                _title, 
                _messageFormat, 
                Constants.Categories.Wpf, 
                DiagnosticSeverity.Warning, 
                isEnabledByDefault: true, 
                description: _description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeClassSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeClassSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            var namedTypeSyntaxNode = (ClassDeclarationSyntax)namedTypeSymbol.DeclaringSyntaxReferences[0].GetSyntax();

            // if not partial return
            if (namedTypeSyntaxNode == null || !namedTypeSyntaxNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
            {
                return;
            }

            var windowParentSymbol = namedTypeSymbol.FindParent(_unexpectedBaseClass);
            if (windowParentSymbol == null)
            {
                return;
            }

            // if is based on Window and PlatformWindow return
            var dialogWindowParentSymbol = namedTypeSymbol.FindParent(_suggestedBaseClass);
            if (dialogWindowParentSymbol != null)
            {
                return;
            }

            // if is not based on PlatformWindow - report
            var diagnostic = Diagnostic.Create(_rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
