using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VS.Dev.Analyzers.Test
{
    [TestClass]
    public class StringEmptyAnalyzerTest : CodeFixVerifier
    {
        [TestMethod]
        public void EmptySourceCode()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void AnalyzeAndFix()
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
    public class ClassWithEmptySttrings
    {
        private const string _constField = """";
        private string _field = """";

        public void SomeMethod(string someParameter = """")
        {
            string localVar = """";
            const string localConst = """";
        }
    }
}";
            var expectedField = new DiagnosticResult
            {
                Id = "StringEmptyLiteral",
                Message = "Replace empty string literal with string.Empty",
                Severity = DiagnosticSeverity.Info,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 14, 33)
                }
            };

            var expectedLocalVar = new DiagnosticResult
            {
                Id = "StringEmptyLiteral",
                Message = "Replace empty string literal with string.Empty",
                Severity = DiagnosticSeverity.Info,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 18, 31)
                }
            };

            VerifyCSharpDiagnostic(test, expectedField, expectedLocalVar);

            var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClassLibrary3
{
    public class ClassWithEmptySttrings
    {
        private const string _constField = """";
        private string _field = string.Empty;

        public void SomeMethod(string someParameter = """")
        {
            string localVar = string.Empty;
            const string localConst = """";
        }
    }
}";
            VerifyCSharpFix(test, fixtest, allowNewCompilerDiagnostics: true);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new StringEmptyLiteralCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new StringEmptyLiteralAnalyzer();
        }
    }
}
