using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace VS.Dev.Analyzers
{
    /// <summary>
    /// If code behind class has constructor with parameters, xaml designer would show error.
    /// Adding a second default constructor without parameters fixes it, however it might 
    /// lead to errors since it might be used in code. Thus the correct pattern for WPF controls
    /// and dialogs is to have only constructor with no parameters (default) and initialize
    /// DataContext by caller after WPF class is initialized. Also it is good practice to have 
    /// ViewModel property that wraps DataContext initialization with Dispatcher calls.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public partial class WpfDefaultConstructorAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WpfDefaultConstructor";

        private static readonly LocalizableString _title = 
            new LocalizableResourceString(
                nameof(Resources.WpfDefaultConstructorAnalyzer_Title), 
                Resources.ResourceManager, 
                typeof(Resources));

        private static readonly LocalizableString _messageFormat = 
            new LocalizableResourceString(
                nameof(Resources.WpfDefaultConstructorAnalyzer_MessageFormat), 
                Resources.ResourceManager, 
                typeof(Resources));

        private static readonly LocalizableString _description = 
            new LocalizableResourceString(
                nameof(Resources.WpfDefaultConstructorAnalyzer_Description), 
                Resources.ResourceManager, 
                typeof(Resources));

        private static readonly DiagnosticDescriptor _rule = 
            new DiagnosticDescriptor(
                DiagnosticId, 
                _title, 
                _messageFormat, 
                Constants.Categories.Wpf, 
                DiagnosticSeverity.Info, 
                isEnabledByDefault: true, 
                description: _description);

        private static readonly string[] _wpfBaseClasses = new[]
        {
            "Microsoft.VisualStudio.PlatformUI.DialogWindow",
            "System.Windows.Window",
            "System.Windows.Controls.UserControl"
        };

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

            // if does not have base class as one of our classes of interest
            var parentSymbol = namedTypeSymbol.FindParent(_wpfBaseClasses);
            if (parentSymbol == null)
            {
                return;
            }

            // check if it has more than one constructor or single constructor has parameters
            if (namedTypeSymbol.Constructors.Length <= 1 && namedTypeSymbol.Constructors[0].Parameters.Length == 0)
            {
                return;
            }

            foreach(var ctorSymbol in namedTypeSymbol.Constructors.Where(x => x.Parameters.Length > 0))
            {
                var diagnostic = Diagnostic.Create(_rule, ctorSymbol.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
