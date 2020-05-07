using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionValidation
{
    internal class DependencyInjectionPoint : ServiceExtensionBase
    {
        public List<Interface> AddedInterfaces { get; private set; }

        public DependencyInjectionPoint(string fully_qualified_name, MethodDeclarationSyntax method)
        {
            FullyQualifiedName = fully_qualified_name;
            Declaration = method;
        }
        internal void FindHandledInterfaces(List<DependencyInjectionPoint> all_dependency_injection_points, List<SimpleServiceExtension> all_service_extensions, List<Interface> interfaces, Compilation compilation)
        {
            if (AddedInterfaces != null)
            {
                return;
            }
            var added_interfaces = new List<Interface>
            {
                Helpers.FindInterface("System.IServiceProvider", interfaces, compilation) // this type is always available for DI
            };
            added_interfaces.AddRange(FindExplicitelyInjectedInterfaces(interfaces, compilation));
            added_interfaces.AddRange(FindAddedInterfaces(all_dependency_injection_points, all_service_extensions, interfaces, compilation));
            AddedInterfaces = added_interfaces;
        }
        private IEnumerable<Interface> FindExplicitelyInjectedInterfaces(List<Interface> interfaces, Compilation compilation)
        {
            return Declaration.AttributeLists
                .SelectMany(a => a.Attributes) // find all attributes
                .Where(attr => attr.Name.ToString() == "Injects") // that are the injects attribute
                .SelectMany(attr => attr.ArgumentList.Arguments) // get their arguments
                .Select(a => a.Expression) // and their expressions
                .OfType<TypeOfExpressionSyntax>() // that are typeof() expressions
                .Select(parameter => FindExplicitelyInjectedInterfaces(parameter, interfaces, compilation))
                .Where(i => i != null);
        }
        private Interface FindExplicitelyInjectedInterfaces(TypeOfExpressionSyntax parameter, List<Interface> interfaces, Compilation compilation)
        {
            var type = Helpers.FindInterface(parameter.Type, interfaces, compilation);
            if (type != null)
            {
                type.Specialise(parameter.Type);
                if (parameter.Type is GenericNameSyntax gns && !gns.TypeArgumentList.Arguments.All(a => string.IsNullOrEmpty(a.ToString())))
                {
                    return type;
                }
            }
            return null;
        }
        private IEnumerable<Interface> FindAddedInterfaces(List<DependencyInjectionPoint> all_dependency_injection_points, List<SimpleServiceExtension> all_service_extensions, List<Interface> interfaces, Compilation compilation)
        {
            var all_member_access_expression = Declaration.Body?.FindMemberAccessExpressions() ?? Declaration.ExpressionBody?.FindMemberAccessExpressions();
            if (all_member_access_expression == null)
            {
                return Enumerable.Empty<Interface>();
            }
            return all_member_access_expression.SelectMany(mae => FindHandledInterfaces(mae, all_dependency_injection_points, all_service_extensions, interfaces, compilation));
        }
        private IEnumerable<Interface> FindHandledInterfaces(MemberAccessExpressionSyntax member_access_expression, List<DependencyInjectionPoint> all_dependency_injection_points, List<SimpleServiceExtension> all_service_extensions, List<Interface> interfaces, Compilation compilation)
        {
            var invocation_name = member_access_expression.Name.Identifier.ToString();
            if (invocation_name == "AddSingleton"
                || invocation_name == "AddScoped"
                || invocation_name == "AddTransient")
            {
                var match = GetInterfaceFromStandardExtensionMethod(member_access_expression, interfaces, compilation);
                if (match != null)
                {
                    yield return match;
                }
            }
            else
            {
                foreach (var match in FindMethodAndHandledInterfaces(member_access_expression.Name, compilation, all_dependency_injection_points, all_service_extensions, interfaces))
                {
                    match.Specialise(member_access_expression.Name);
                    yield return match;
                }
            }
        }
        private IEnumerable<Interface> FindMethodAndHandledInterfaces(TypeSyntax type, Compilation compilation, List<DependencyInjectionPoint> all_dependency_injection_points, List<SimpleServiceExtension> all_service_extensions, List<Interface> interfaces)
        {
            var method_name = (type is GenericNameSyntax gns) ? gns.Identifier.ToString() : type.ToString();
            var candidate_namespaces = type.GetCandidateNamespaces();
            var matching_dependency_injection_point = all_dependency_injection_points.FirstOrDefault(se => se.FullyQualifiedName.EndsWith(method_name) && candidate_namespaces.Any(cn => se.FullyQualifiedName.StartsWith(cn)));
            if (matching_dependency_injection_point != null)
            {
                matching_dependency_injection_point.FindHandledInterfaces(all_dependency_injection_points, all_service_extensions, interfaces, compilation);
                foreach (var result in matching_dependency_injection_point.AddedInterfaces)
                {
                    yield return result;
                }
                yield break;
            }
            var matching_service_extension = all_service_extensions.FirstOrDefault(se => se.FullyQualifiedName.EndsWith(method_name) && candidate_namespaces.Any(cn => se.FullyQualifiedName.StartsWith(cn)));
            if (matching_service_extension != null)
            {
                matching_service_extension.FindHandledInterfaces(all_service_extensions, interfaces, compilation, include_ignored: false);
                foreach (var result in matching_service_extension.AddedInterfaces)
                {
                    yield return result;
                }
                yield break;
            }

            foreach (var exposed_interface in compilation.FindExposedInterfacesOfServiceExtension(method_name, candidate_namespaces))
            {
                yield return exposed_interface;
            }
        }
    }
}