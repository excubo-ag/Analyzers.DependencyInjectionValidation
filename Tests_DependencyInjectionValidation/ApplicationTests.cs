using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using TestHelper;

namespace DependencyInjectionValidation.Test
{
    [TestClass]
    public class ApplicationTests : CodeFixVerifier
    {
        private static string path_prefix = FindPathPrefix("App");
        [DataTestMethod]
        [DataRow("DependencyExternal")]
        public void NoDiagnostic(string folder_name)
        {
            var all_files = GetAllFileContents($"{path_prefix}/NoDiagnostic/{folder_name}").ToArray();
            VerifyCSharpDiagnostic(all_files);
        }
        [DataTestMethod]
        [DataRow("PublicWorker")]
        public void MissingConstructorDependency(string folder_name)
        {
            var all_filenames = GetAllFilenames($"{path_prefix}/MissingConstructorDependency/{folder_name}").ToArray();
            var all_files = all_filenames.Select(f => File.ReadAllText(f)).ToArray();
            var missing_interface = all_filenames[0].Split('/', '\\').Last().Replace(".cs", "");
            VerifyCSharpDiagnostic(all_files, new DiagnosticResult
            {
                Id = "EDI04",
                Message = string.Format("Dependency {0} of {1} is missing.", missing_interface, $"DependencyInjectionValidation.Test.{folder_name}"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 18)
                }
            });
        }
        [DataTestMethod]
        [DataRow("InternalDependency")]
        public void MissingDependencyBase(string folder_name)
        {
            var all_filenames = GetAllFilenames($"{path_prefix}/MissingDependencyBase/{folder_name}").ToArray();
            var all_files = all_filenames.Select(f => File.ReadAllText(f)).ToArray();
            var missing_interface = all_filenames[0].Split('/', '\\').Last().Replace(".cs", "");
            VerifyCSharpDiagnostic(all_files, new DiagnosticResult
            {
                Id = "EDI04",
                Message = string.Format("Dependency {0} of {1} is missing.", missing_interface, $"DependencyInjectionValidation.Test.{folder_name}"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 20)
                }
            });
        }
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DependencyInjectionValidationAnalyzer();
        }
    }
}