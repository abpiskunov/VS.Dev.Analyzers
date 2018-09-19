using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VS.Dev.Analyzers.Test
{
    [TestClass]
    public class WpfDefaultConstructorAnalyzerTests : CodeFixVerifier
    {
        private static readonly string[] _wpfBaseClasses = new[]
        {
            "Microsoft.VisualStudio.PlatformUI.DialogWindow",
            "System.Windows.Window",
            "System.Windows.Controls.UserControl"
        };

        [TestMethod]
        public void EmptySourceCode()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Analyze_Partial_WithExpectedBase()
        {
            const string test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClassLibrary3
{
    public partial class MyClass : {0}
    {
        public MyClass(string x)
        {
        }
    }
}";
            foreach (var baseClass in _wpfBaseClasses)
            {
                var testWithBaseClass = test.Replace("{0}", baseClass);

                var expected = new DiagnosticResult
                {
                    Id = "WpfDefaultConstructor",
                    Message = "For xaml designer to work properly code behind classes should have only default constructor without parameters.",
                    Severity = DiagnosticSeverity.Info,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 13, 16)
                    }
                };

                VerifyCSharpDiagnostic(testWithBaseClass, expected);
            }
        }

        [TestMethod]
        public void Analyze_Partial_With2Ctors()
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
    public partial class MyClass : {0}
    {
        public MyClass(string x)
        {
        }

        public MyClass()
        {
        }
    }
}";
            foreach (var baseClass in _wpfBaseClasses)
            {
                var testWithBaseClass = test.Replace("{0}", baseClass);

                var expected = new DiagnosticResult
                {
                    Id = "WpfDefaultConstructor",
                    Message = "For xaml designer to work properly code behind classes should have only default constructor without parameters.",
                    Severity = DiagnosticSeverity.Info,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 13, 16)
                    }
                };

                VerifyCSharpDiagnostic(testWithBaseClass, expected);
            }
        }

        [TestMethod]
        public void Analyze_Partial_WithExpectedAncestor()
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
    public partial class MyBase : {0}
    {
    }

    public partial class MyClass : MyBase
    {
        public MyClass(string x)
        {
        }

        public MyClass()
        {
        }
    }
}";
            foreach (var baseClass in _wpfBaseClasses)
            {
                var testWithBaseClass = test.Replace("{0}", baseClass);

                var expected = new DiagnosticResult
                {
                    Id = "WpfDefaultConstructor",
                    Message = "For xaml designer to work properly code behind classes should have only default constructor without parameters.",
                    Severity = DiagnosticSeverity.Info,
                    Locations = new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", 17, 16)
                    }
                };

                VerifyCSharpDiagnostic(testWithBaseClass, expected);
            }
        }

        [TestMethod]
        public void Analyze_NotPartial()
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
    public class MyClass : Microsoft.VisualStudio.PlatformUI.DialogWindow
    {
        public MyClass(string x)
        {
        }
    }
}";
            foreach (var baseClass in _wpfBaseClasses)
            {
                VerifyCSharpDiagnostic(test.Replace("{0}", baseClass));
            }
        }

        [TestMethod]
        public void Analyze_Partial_NoCtors()
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
    public class MyClass : Microsoft.VisualStudio.PlatformUI.DialogWindow
    {
    }
}";
            foreach (var baseClass in _wpfBaseClasses)
            {
                VerifyCSharpDiagnostic(test.Replace("{0}", baseClass));
            }
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new WpfDefaultConstructorAnalyzer();
        }
    }
}
