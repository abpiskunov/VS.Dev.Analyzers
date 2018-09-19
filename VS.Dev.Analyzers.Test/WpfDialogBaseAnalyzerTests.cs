using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VS.Dev.Analyzers.Test
{
    [TestClass]
    public class WpfDialogBaseAnalyzerTests : CodeFixVerifier
    {
        [TestMethod]
        public void EmptySourceCode()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Analyze_PartialWithBaseWindow()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClassLibrary3
{
    public partial class ClassWithEmptySttrings : System.Windows.Window
    {
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "WpfDialogBase",
                Message = "Dialogs in VisualStudio should be based on Microsoft.VisualStudio.PlatformUI.DialogWindow",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 26)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void Analyze_PartialWithAncestorWindow()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClassLibrary3
{
    public class Base : System.Windows.Window
    {
    }

    public partial class ClassWithEmptySttrings : Base
    {
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "WpfDialogBase",
                Message = "Dialogs in VisualStudio should be based on Microsoft.VisualStudio.PlatformUI.DialogWindow",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 15, 26)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void Analyze_NotPartialWithBaseWindow()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClassLibrary3
{
    public class ClassWithEmptySttrings : System.Windows.Window
    {
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Analyze_PartialWithBaseNotWindow()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClassLibrary3
{
    public partial class ClassWithEmptySttrings : object
    {
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Analyze_PartialWithBaseDialogWindow()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClassLibrary3
{
    public partial class ClassWithEmptySttrings : Microsoft.VisualStudio.PlatformUI.DialogWindow
    {
    }
}";
            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new WpfDialogBaseAnalyzer();
        }
    }
}
