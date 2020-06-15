using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using TestHelper;

namespace DependencyInjectionValidation.Test
{
    [TestClass]
    public class WithDependencyOnOtherAssemblyTests : CodeFixVerifier
    {
        private const string path_prefix = "../../../TestCases/OA";
        [DataTestMethod]
        [DataRow("DependingOnOtherAssembly")]
        [DataRow("DependingOnOtherAssemblyAndImplementingLocal")]
        [DataRow("DependingOnOtherAssemblyAndImplementingLocal_A1_A2_")]
        [DataRow("DependingOnOtherAssemblyAndImplementingLocal_Free")]
        [DataRow("DependingOnOtherAssemblyAndImplementingLocal_string_object_")]
        [DataRow("ImplementationOfInterfaceFromOtherAssembly")]
        [DataRow("ImplementationOfInterfaceFromOtherAssemblyFullyQualified")]
        [DataRow("ImplementationOfInterfaceFromOtherAssemblyWithDependency")]
        [DataRow("InheritJustClass")]
        public void NoDiagnostic(string folder_name)
        {
            var all_files = GetAllFilenames($"{path_prefix}/NoDiagnostic/{folder_name}").Select(f => File.ReadAllText(f)).ToArray();
            VerifyCSharpDiagnostic(all_files);
        }
        [DataTestMethod]
        [DataRow("DependingOnOtherAssemblyAndImplementingLocal")]
        [DataRow("DependingOnOtherAssemblyAndImplementingLocal_string_object_")]
        [DataRow("ImplementationOfInterfaceFromOtherAssemblyWithDependency")]
        public void TooManyServiceExtensions(string folder_name)
        {
            var all_files = GetAllFilenames($"{path_prefix}/TooManyServiceExtensions/{folder_name}").Select(f => File.ReadAllText(f)).ToArray();
            VerifyCSharpDiagnostic(all_files, new DiagnosticResult
            {
                Id = "EDI01",
                Message = string.Format("Too many service extensions for class {0}. Candidates are: {1}.", $"My.Namespace.{folder_name}", "My.Namespace.ServiceExtension.AddService1, My.Namespace.ServiceExtension.AddService2"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 20)
                }
            });
        }
        [DataTestMethod]
        [DataRow("DependingOnOtherAssemblyAndImplementingLocal_B1_B2_")]
        public void TooManyServiceExtensionsGeneric(string folder_name)
        {
            var all_files = GetAllFilenames($"{path_prefix}/TooManyServiceExtensions/{folder_name}").Select(f => File.ReadAllText(f)).ToArray();
            VerifyCSharpDiagnostic(all_files, new DiagnosticResult
            {
                Id = "EDI01",
                Message = string.Format("Too many service extensions for class {0}. Candidates are: {1}.", $"My.Namespace.{folder_name}", "My.Namespace.ServiceExtension.AddService1<B1,B2>, My.Namespace.ServiceExtension.AddService2<B1,B2>"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 20)
                }
            });
        }
        [DataTestMethod]
        [DataRow("DependingOnOtherAssemblyAndImplementingLocal")]
        [DataRow("DependingOnOtherAssemblyAndImplementingLocal_A1_A2_")]
        [DataRow("ImplementationOfInterfaceFromOtherAssemblyWithDependency")]
        public void MissingServiceExtension(string folder_name)
        {
            var all_files = GetAllFilenames($"{path_prefix}/MissingServiceExtension/{folder_name}").Select(f => File.ReadAllText(f)).ToArray();
            VerifyCSharpDiagnostic(all_files, new DiagnosticResult
            {
                Id = "EDI02",
                Message = string.Format("Missing service extension for class {0}.", $"My.Namespace.{folder_name}"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 10, 20)
                }
            });
        }
        [DataTestMethod]
        [DataRow("DependingOnOtherAssemblyAndImplementingLocal")]
        [DataRow("DependingOnOtherAssemblyAndImplementingLocal_B1_B2_")]
        [DataRow("DependingOnOtherAssemblyAndImplementingLocal_string_object_")]
        [DataRow("Impl")]
        public void MissingDependencyBase(string folder_name)
        {
            var all_filenames = GetAllFilenames($"{path_prefix}/MissingDependencyBase/{folder_name}").ToArray();
            var all_files = all_filenames.Select(f => File.ReadAllText(f)).ToArray();
            var missing_interface = all_filenames[0].Split('/', '\\').Last().Replace(".cs", "");
            VerifyCSharpDiagnostic(all_files, new DiagnosticResult
            {
                Id = "EDI03",
                Message = string.Format("Service extension is not adding all required interfaces for {0}. Missing interface: {1}.", $"My.Namespace.{folder_name}", missing_interface),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 19, 42)
                }
            });
        }
        [DataTestMethod]
        [DataRow("C")]
        [DataRow("C_B1_B2_")]
        [DataRow("C_A_B_")]
        [DataRow("ImplementationOfInterfaceFromOtherAssemblyWithDependency")]
        public void MissingConstructorDependency(string folder_name)
        {
            var all_filenames = GetAllFilenames($"{path_prefix}/MissingConstructorDependency/{folder_name}").ToArray();
            var all_files = all_filenames.Select(f => File.ReadAllText(f)).ToArray();
            var missing_interface = all_filenames[0].Split('/', '\\').Last().Replace(".cs", "");
            VerifyCSharpDiagnostic(all_files, new DiagnosticResult
            {
                Id = "EDI03",
                Message = string.Format("Service extension is not adding all required interfaces for {0}. Missing interface: {1}.", $"My.Namespace.{folder_name}", missing_interface),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 19, 42)
                }
            });
        }
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DependencyInjectionValidationAnalyzer();
        }
    }
}
