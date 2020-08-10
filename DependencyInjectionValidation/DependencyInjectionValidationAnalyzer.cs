using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DependencyInjectionValidation
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DependencyInjectionValidationAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DependencyInjectionValidation";

        private static readonly LocalizableString MissingServiceExtensionTitle = new LocalizableResourceString(nameof(Resources.MissingServiceExtensionAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingServiceExtensionMessageFormat = new LocalizableResourceString(nameof(Resources.MissingServiceExtensionAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingServiceExtensionDescription = new LocalizableResourceString(nameof(Resources.MissingServiceExtensionAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingDependencyTitle = new LocalizableResourceString(nameof(Resources.MissingDependencyAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingDependencyMessageFormat = new LocalizableResourceString(nameof(Resources.MissingDependencyAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingDependencyDescription = new LocalizableResourceString(nameof(Resources.MissingDependencyAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingDependencyInApplicationTitle = new LocalizableResourceString(nameof(Resources.MissingDependencyInApplicationTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingDependencyInApplicationMessageFormat = new LocalizableResourceString(nameof(Resources.MissingDependencyInApplicationMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MissingDependencyInApplicationDescription = new LocalizableResourceString(nameof(Resources.MissingDependencyInApplicationDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString TooManyServiceExtensionsTitle = new LocalizableResourceString(nameof(Resources.TooManyServiceExtensionsTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString TooManyServiceExtensionsMessageFormat = new LocalizableResourceString(nameof(Resources.TooManyServiceExtensionsMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString TooManyServiceExtensionsDescription = new LocalizableResourceString(nameof(Resources.TooManyServiceExtensionsDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Dependency Injection";

        private static readonly DiagnosticDescriptor TooManyServiceExtensionsRule = new DiagnosticDescriptor("EDI01", TooManyServiceExtensionsTitle, TooManyServiceExtensionsMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: TooManyServiceExtensionsDescription);
        private static readonly DiagnosticDescriptor MissingServiceExtensionRule = new DiagnosticDescriptor("EDI02", MissingServiceExtensionTitle, MissingServiceExtensionMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: MissingServiceExtensionDescription);
        private static readonly DiagnosticDescriptor MissingDependencyRule = new DiagnosticDescriptor("EDI03", MissingDependencyTitle, MissingDependencyMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: MissingDependencyDescription);
        private static readonly DiagnosticDescriptor MissingDependencyInApplicationRule = new DiagnosticDescriptor("EDI04", MissingDependencyInApplicationTitle, MissingDependencyInApplicationMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: MissingDependencyInApplicationDescription);
        //private static readonly DiagnosticDescriptor DebuggingRule = new DiagnosticDescriptor("INTERNAL", "DEBUGGING", "{0}", Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "");
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(MissingServiceExtensionRule, MissingDependencyRule, TooManyServiceExtensionsRule, MissingDependencyInApplicationRule
//    , DebuggingRule
);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterCompilationAction(CompilationDoneAction);
        }
        private void CompilationDoneAction(CompilationAnalysisContext context)
        {
            var all_interfaces = context
                .Compilation
                .SyntaxTrees
                .SelectMany(syntax_tree => syntax_tree.GetRoot().FindInterfaces())
                .ToList();
            #region exit if cancelled
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            #endregion
            var all_classes = context
                .Compilation
                .SyntaxTrees
                .SelectMany(syntax_tree => syntax_tree.GetRoot().FindClasses())
                .Where(c => !c.IsPrivate)
                .ToList();
            #region exit if cancelled
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            #endregion
            var classes_with_dependencies = all_classes
                .Select(@class =>
                {
                    foreach (var constructor in @class.Constructors)
                    {
                        constructor.FindDependencies(all_interfaces, context.Compilation);
                    }
                    @class.FindBases(all_interfaces, context.Compilation);
                    @class.FindProperties(all_interfaces, context.Compilation);
                    return @class;
                })
                .ToList();
            #region exit if cancelled
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            #endregion
            if (!all_classes.Any())
            {
                return;
            }
            var extension_methods = all_classes
                .Where(@class => @class.IsStatic)
                .SelectMany(@class => (@class.Declaration as ClassDeclarationSyntax)
                        .FindMethods()
                        .Where(method => method.ParameterList != null && method.ParameterList.Parameters.Any())
                        .Where(method => method.ParameterList.Parameters.Any(p => p.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword)))) // this is an extension method
                        .Where(method => method.ParameterList.Parameters.First().Type.ToString().EndsWith("IServiceCollection")) // this is a service extension method
                        .Where(method => method.AttributeLists.Any(list => list.Attributes.Any(a => a.Name.ToString().StartsWith("Exposes")))) // this is a SE that exposes something
                        .Select(method => new ServiceExtension(@class.FullyQualifiedName, method))
                ).ToList();
            #region exit if cancelled
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            #endregion
            AddHandledTypes(extension_methods, classes_with_dependencies, context.Compilation);
            #region exit if cancelled
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            #endregion
            var dependency_injection_points = all_classes
                .SelectMany(@class => (@class.Declaration as ClassDeclarationSyntax)
                        .FindMethods()
                        .Where(method => method.ParameterList != null && method.ParameterList.Parameters.Any())
                        .Where(method => method.ParameterList.Parameters.First().Type.ToString().EndsWith("IServiceCollection")) // this is a method handling IServiceCollection
                        .Where(method => method.AttributeLists.Any(list => list.Attributes.Any(a => a.Name.ToString().StartsWith("DependencyInjectionPoint")))) // this is a DIP
                        .Select(method => new DependencyInjectionPoint(@class.FullyQualifiedName, method)))
                .ToList();
            if (dependency_injection_points.Any())
            {
                // this is an application project. We do not require service extensions from now on
                AnalyzeAsApplication(context, extension_methods, classes_with_dependencies, all_interfaces, dependency_injection_points);
            }
            else
            {
                AnalyzeAsLibrary(context, extension_methods, classes_with_dependencies, all_interfaces);
            }
        }

        private void AnalyzeAsApplication(CompilationAnalysisContext context, List<ServiceExtension> extension_methods, List<Class> classes_with_dependencies, List<Interface> all_interfaces, List<DependencyInjectionPoint> dependency_injection_points)
        {
            var simple_di_extension_methods = extension_methods.SelectMany(method => method.HandledTypes.Select(ht => new SimpleServiceExtension(method, ht))).ToList();
            var required_additions = classes_with_dependencies.Where(@class => @class.RequiresDependencyInjection()).ToList();
            foreach (var dip in dependency_injection_points)
            {
                dip.FindHandledInterfaces(dependency_injection_points, simple_di_extension_methods, all_interfaces, context.Compilation);
            }
            var fulfilled_dependencies = dependency_injection_points.SelectMany(dip => dip.AddedInterfaces).ToList();
            foreach (var @class in required_additions)
            {
                var dependencies = @class.RequiredInterfaces().ToList();
                var missing_dependencies = dependencies.Where(r => !fulfilled_dependencies.Any(i =>
                    i.FullyQualifiedName == r.FullyQualifiedName ||
                    (i.FullyQualifiedName == i.GenericTypeName && i.GenericTypeName == r.GenericTypeName)
                    )).ToList();
                foreach (var missing_dependency in missing_dependencies)
                {
                    context.ReportDiagnostic(Diagnostic.Create(MissingDependencyInApplicationRule, @class.Declaration.Identifier.GetLocation(), missing_dependency.FullyQualifiedName, @class.FullyQualifiedName));
                }
            }
        }

        private void AnalyzeAsLibrary(CompilationAnalysisContext context, List<ServiceExtension> extension_methods, List<Class> classes_with_dependencies, List<Interface> all_interfaces)
        {
            var classes_with_too_many_extension_methods = extension_methods
                .SelectMany(se => se.HandledTypes.Select(ht => new SimpleServiceExtension(se, ht)))
                .GroupBy(se => se.HandledType.FullyQualifiedName).Where(e => e.Count() > 1).Select(e => (ClassName: e.Key, Extensions: e.ToList())) // find those that appear multiple times
                .Select(e => (FullyQualifiedName: e.ClassName, Class: classes_with_dependencies.Where(c => c.GenericTypeName == e.Extensions.First().HandledType.GenericTypeName).Select(c => c.Declaration).FirstOrDefault(), Extensions: e.Extensions))
                .Where(e => e.Class != null)
                .ToList();
            foreach (var @class_with_many_extensions in classes_with_too_many_extension_methods)
            {
                context.ReportDiagnostic(Diagnostic.Create(TooManyServiceExtensionsRule, @class_with_many_extensions.Class.Identifier.GetLocation(), @class_with_many_extensions.FullyQualifiedName, string.Join(", ", class_with_many_extensions.Extensions.Select(e => e.FullyQualifiedName))));
            }
            #region exit if cancelled
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            #endregion
            var missing_di_extension_classes = GetClassesThatMissDiExtension(classes_with_dependencies, extension_methods).ToList();
            foreach (var @class in missing_di_extension_classes)
            {
                context.ReportDiagnostic(Diagnostic.Create(MissingServiceExtensionRule, @class.Declaration.Identifier.GetLocation(), @class.FullyQualifiedName));
            }
            #region exit if cancelled
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            #endregion
            var remaining_classes = classes_with_dependencies
                .Where(@class => extension_methods.SelectMany(e => e.HandledTypes).Any(ht => ht.GenericTypeName == @class.GenericTypeName))
                .ToList();
            #region exit if cancelled
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            #endregion
            var simple_di_extension_methods = extension_methods.SelectMany(method => method.HandledTypes.Select(ht => new SimpleServiceExtension(method, ht))).ToList();
            #region exit if cancelled
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            #endregion
            var incomplete_di_extensions = GetIncompleteExtensionMethods(remaining_classes, all_interfaces, simple_di_extension_methods, context.Compilation).ToList();
            foreach (var service_extension in incomplete_di_extensions)
            {
                foreach (var missing_interface in service_extension.MissingInterfaces)
                {
                    context.ReportDiagnostic(Diagnostic.Create(MissingDependencyRule, service_extension.Declaration.Identifier.GetLocation(), service_extension.HandledType, missing_interface));
                }
            }
        }

        private static IEnumerable<Class> GetClassesThatMissDiExtension(List<Class> classes, List<ServiceExtension> extension_methods)
        {
            return classes.Where(@class => @class.RequiresServiceExtension() && !@class.HasServiceExtension(extension_methods));
        }
        private static void AddHandledTypes(List<ServiceExtension> di_extension_methods, List<Class> internal_classes, Compilation compilation)
        {
            foreach (var method in di_extension_methods)
            {
                foreach (var di_attribute in method.Declaration.AttributeLists.SelectMany(l => l.Attributes).Where(a => a.Name.ToString().StartsWith("Exposes")))
                {
                    var arguments = di_attribute.ArgumentList.Arguments;
                    foreach (var argument in arguments.Where(a => a.Expression is TypeOfExpressionSyntax).Select(a => a.Expression).Cast<TypeOfExpressionSyntax>())
                    {
                        var match = Helpers.FindClass(argument.Type, internal_classes, compilation);
                        if (match != null)
                        {
                            method.HandledTypes.Add(match);
                        }
                    }
                }
            }
        }
        private static IEnumerable<SimpleServiceExtension> GetIncompleteExtensionMethods(List<Class> classes, List<Interface> interfaces, List<SimpleServiceExtension> service_extensions, Compilation compilation)
        {
            if (!classes.Any())
            {
                return Enumerable.Empty<SimpleServiceExtension>();
            }
            foreach (var service_extension in service_extensions)
            {
                service_extension.FindHandledInterfaces(service_extensions, interfaces, compilation);
            }
            foreach (var service_extension in service_extensions)
            {
                var handled_classes = classes
                    .Where(c => service_extension.HandledType.GenericTypeName == c.GenericTypeName)
                    .Select(c => c.Instantiate(service_extension.HandledType.TypeArguments))
                    .ToList();
                // here we need to "instantiate" the right class types. the service extension might handle a specific type
                service_extension.FindRequiredInterfaces(handled_classes);
            }
            foreach (var service_extension in service_extensions)
            {
                service_extension.FindMissingInterfaces();
            }
            return service_extensions.Where(se => se.MissingInterfaces.Any());
        }
    }
}
