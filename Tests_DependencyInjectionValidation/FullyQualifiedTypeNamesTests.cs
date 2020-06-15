using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using TestHelper;

namespace DependencyInjectionValidation.Test
{
    [TestClass]
    public class FullyQualifiedTypeNamesTests : CodeFixVerifier
    {
        private const string path_prefix = "../../../TestCases/FQ";
        [DataTestMethod]
        [DataRow("Empty")]
        [DataRow("DependingOnServiceInInternalConstructor")]
        [DataRow("DependingOnServiceInPrivateConstructor")]
        [DataRow("DependingOnServiceInPrivateConstructor_U1U2")]
        [DataRow("DependingOnServiceInPublicConstructor")]
        [DataRow("DependingOnServiceInPublicConstructor_U1U2")]
        [DataRow("DependingOnServiceInPublicConstructor_U1U2_Fixed")]
        [DataRow("DependingOnServiceInPublicConstructor_U1U2_ManyExtensions")]
        [DataRow("ImplicitUse")]
        [DataRow("ImplicitUse_U")]
        [DataRow("ImplicitUse_U1U2")]
        [DataRow("InternalUse")]
        [DataRow("InternalUse_U")]
        [DataRow("InternalUse_U1U2")]
        [DataRow("PublicUse")]
        [DataRow("PublicUse_U")]
        [DataRow("PublicUse_U1U2")]
        public void NoDiagnostic(string folder_name)
        {
            var all_files = GetAllFilenames($"{path_prefix}/NoDiagnostic/{folder_name}").Select(f => File.ReadAllText(f)).ToArray();
            VerifyCSharpDiagnostic(all_files);
        }
        [DataTestMethod]
        [DataRow("DependingOnServiceInInternalConstructor")]
        [DataRow("DependingOnServiceInPublicConstructor")]
        [DataRow("DependingOnServiceInPublicConstructor_string_object_")]
        public void TooManyServiceExtensions(string folder_name)
        {
            var all_files = GetAllFilenames($"{path_prefix}/TooManyServiceExtensions/{folder_name}").Select(f => File.ReadAllText(f)).ToArray();
            VerifyCSharpDiagnostic(all_files, new DiagnosticResult
            {
                Id = "EDI01",
                Message = string.Format("Too many service extensions for class {0}. Candidates are: {1}.", $"Types.{folder_name}", "Types.ServiceExtension.AddService1, Types.ServiceExtension.AddService2"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {   
                    new DiagnosticResultLocation("Test0.cs", 37, 18)
                }
            });
        }
        [DataTestMethod]
        [DataRow("DependingOnServiceInPublicConstructor_U1_U2_")]
        public void TooManyServiceExtensionsGeneric(string folder_name)
        {
            var all_files = GetAllFilenames($"{path_prefix}/TooManyServiceExtensions/{folder_name}").Select(f => File.ReadAllText(f)).ToArray();
            VerifyCSharpDiagnostic(all_files, new DiagnosticResult
            {
                Id = "EDI01",
                Message = string.Format("Too many service extensions for class {0}. Candidates are: {1}.", $"Types.{folder_name}", "Types.ServiceExtension.AddService1<U1,U2>, Types.ServiceExtension.AddService2<U1,U2>"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 37, 18)
                }
            });
        }
        [DataTestMethod]
        [DataRow("DependingOnServiceInInternalConstructor")]
        [DataRow("DependingOnServiceInPublicConstructor")]
        [DataRow("DependingOnServiceInPublicConstructor_U1_U2_")]
        public void MissingServiceExtension(string folder_name)
        {
            var all_files = GetAllFilenames($"{path_prefix}/MissingServiceExtension/{folder_name}").Select(f => File.ReadAllText(f)).ToArray();
            VerifyCSharpDiagnostic(all_files, new DiagnosticResult
            {
                Id = "EDI02",
                Message = string.Format("Missing service extension for class {0}.", $"Types.{folder_name}"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 37, 18)
                }
            });
        }
        [DataTestMethod]
        [DataRow("DependingOnServiceInInternalConstructor")]
        [DataRow("DependingOnServiceInPublicConstructor")]
        [DataRow("DependingOnServiceInPublicConstructor_U1_U2_")]
        [DataRow("DependingOnServiceInPublicConstructor_string_object_")]
        [DataRow("DependingOnServiceInPublicConstructor_object_string_")]
        [DataRow("ImplicitUse")]
        [DataRow("ImplicitUse_U_")]
        [DataRow("ImplicitUse_U1_U2_")]
        public void MissingDependencyBase(string folder_name)
        {
            var all_filenames = GetAllFilenames($"{path_prefix}/MissingDependencyBase/{folder_name}").ToArray();
            var all_files = all_filenames.Select(f => File.ReadAllText(f)).ToArray();
            var missing_interface = all_filenames[0].Split('/', '\\').Last().Replace(".cs", "");
            VerifyCSharpDiagnostic(all_files, new DiagnosticResult
            {
                Id = "EDI03",
                Message = string.Format("Service extension is not adding all required interfaces for {0}. Missing interface: {1}.", $"Types.{folder_name}", missing_interface),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 47, 42)
                }
            });
        }
        [DataTestMethod]
        [DataRow("DependingOnServiceInInternalConstructor")]
        [DataRow("DependingOnServiceInPublicConstructor")]
        [DataRow("DependingOnServiceInPublicConstructor_object_string_")]
        [DataRow("DependingOnServiceInPublicConstructor_string_object_")]
        [DataRow("DependingOnServiceInPublicConstructor_U1_U2_")]
        public void MissingConstructorDependency(string folder_name)
        {
            var all_filenames = GetAllFilenames($"{path_prefix}/MissingConstructorDependency/{folder_name}").ToArray();
            var all_files = all_filenames.Select(f => File.ReadAllText(f)).ToArray();
            var missing_interface = all_filenames[0].Split('/', '\\').Last().Replace(".cs", "");
            VerifyCSharpDiagnostic(all_files, new DiagnosticResult
            {
                Id = "EDI03",
                Message = string.Format("Service extension is not adding all required interfaces for {0}. Missing interface: {1}.", $"Types.{folder_name}", missing_interface),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 47, 42)
                }
            });
        }
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DependencyInjectionValidationAnalyzer();
        }
    }
}
