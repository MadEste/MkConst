using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using MkConst;
using Verify = Microsoft.CodeAnalysis.CSharp.CodeFix.Testing.MSTest.CodeFixVerifier<
    MkConst.MkConstAnalyzer,
    MkConst.MkConstCodeFixProvider>;

namespace MkConst.Test
{
    [TestClass]
    public class UnitTest
    {
        private const string LocalIntCouldBeConstant = @"
        using System;

        namespace MakeConstTest
        {
            class Program
            {
                static void Main(string[] args)
                {
                    int i = 0;
                    Console.WriteLine(i);
                }
            }
        }";

                private const string LocalIntCouldBeConstantFixed = @"
        using System;

        namespace MakeConstTest
        {
            class Program
            {
                static void Main(string[] args)
                {
                    const int i = 0;
                    Console.WriteLine(i);
                }
            }
        }";
        //No diagnostics expected to show up
        [DataTestMethod]
        [DataRow("")]
        public void WhenTestCodeIsValidNoDiagnosticIsTriggered(string testCode)
        {
            VerifyCSharpDiagnostic(testCode);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [DataTestMethod]
        [DataRow(LocalIntCouldBeConstant, LocalIntCouldBeConstantFixed, 10, 13)]
        public void WhenDiagnosticIsRaisedFixUpdatesCode(
            string test,
            string fixTest,
            int line,
            int column)
        {
            var expected = new DiagnosticResult
            {
                Id = MakeConstAnalyzer.DiagnosticId,
                Message = new LocalizableResourceString(nameof(MakeConst.Resources.AnalyzerMessageFormat), MakeConst.Resources.ResourceManager, typeof(MakeConst.Resources)).ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                    new DiagnosticResultLocation("Test0.cs", line, column)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            VerifyCSharpFix(test, fixTest);
        }
    }
}
