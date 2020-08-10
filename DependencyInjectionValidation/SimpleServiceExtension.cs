using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionValidation
{
    internal class SimpleServiceExtension : ServiceExtensionBase
    {
        public Class OriginalHandledType { get; }
        public Class HandledType { get; }
        internal List<Interface> AlwaysAvailableInterfaces { get; set; }
        internal List<Interface> ExplicitelyInjectedInterfaces { get; set; }
        internal List<Interface> ExplicitelyIgnoredInterfaces { get; set; }
        internal List<Interface> RecursivelyDiscoveredInterfaces { get; set; }
        internal IEnumerable<Interface> AddedInterfaces =>
            ExplicitelyInjectedInterfaces == null || RecursivelyDiscoveredInterfaces == null || AlwaysAvailableInterfaces == null || ExplicitelyIgnoredInterfaces == null
            ? null
            : AlwaysAvailableInterfaces.Concat(ExplicitelyInjectedInterfaces).Concat(ExplicitelyIgnoredInterfaces).Concat(RecursivelyDiscoveredInterfaces).Where(i => i != null);

        public List<Interface> RequiredInterfaces { get; private set; }
        public List<Interface> MissingInterfaces { get; private set; }
        public SimpleServiceExtension(ServiceExtension service_extension, Class handled_type)
        {
            (FullyQualifiedName, Declaration) = (service_extension.FullyQualifiedName, service_extension.Declaration);
            OriginalHandledType = handled_type;
            HandledType = handled_type.Instantiate(TypeArguments);
        }
        internal void FindRequiredInterfaces(List<Class> classes)
        {
            RequiredInterfaces = classes.SelectMany(@class => @class.RequiredInterfaces()).ToList();
        }
        internal void FindHandledInterfaces(List<SimpleServiceExtension> all_service_extensions, List<Interface> interfaces, Compilation compilation, bool include_ignored = true)
        {
            if (AddedInterfaces != null)
            {
                return;
            }
            AlwaysAvailableInterfaces = new List<Interface>
            {
                Helpers.FindInterface("System.IServiceProvider", interfaces, compilation),
                Helpers.FindInterface("System.IDisposable", interfaces, compilation)
            };
            ExplicitelyInjectedInterfaces = FindExplicitelyInjectedInterfaces(interfaces, compilation).ToList();
            if (include_ignored)
            {
                ExplicitelyIgnoredInterfaces = FindExplicitelyIgnoredInterfaces(interfaces, compilation).ToList();
            }
            else
            {
                ExplicitelyIgnoredInterfaces = new List<Interface>();
            }
            RecursivelyDiscoveredInterfaces = FindAddedInterfaces(all_service_extensions, interfaces, compilation).ToList();
        }
        private IEnumerable<Interface> FindAddedInterfaces(List<SimpleServiceExtension> all_service_extensions, List<Interface> interfaces, Compilation compilation)
        {
            var all_member_access_expression = Declaration.Body?.FindMemberAccessExpressions() ?? Declaration.ExpressionBody?.FindMemberAccessExpressions();
            if (all_member_access_expression == null)
            {
                return Enumerable.Empty<Interface>();
            }
            return all_member_access_expression.SelectMany(mae => FindHandledInterfaces(mae, all_service_extensions, interfaces, compilation));
        }
        private IEnumerable<Interface> FindHandledInterfaces(MemberAccessExpressionSyntax member_access_expression, List<SimpleServiceExtension> all_service_extensions, List<Interface> interfaces, Compilation compilation)
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
                foreach (var match in FindMethodAndHandledInterfaces(member_access_expression.Name, compilation, all_service_extensions, interfaces))
                {
                    match.Specialise(member_access_expression.Name);
                    yield return match;
                }
            }
        }
        private IEnumerable<Interface> FindMethodAndHandledInterfaces(TypeSyntax type, Compilation compilation, List<SimpleServiceExtension> all_service_extensions, List<Interface> interfaces)
        {
            var method_name = (type is GenericNameSyntax gns) ? gns.Identifier.ToString() : type.ToString();
            var candidate_namespaces = type.GetCandidateNamespaces();
            var matching_service_extension = all_service_extensions.FirstOrDefault(se => se.FullyQualifiedName.EndsWith(method_name) && candidate_namespaces.Any(cn => se.FullyQualifiedName.StartsWith(cn)));
            if (matching_service_extension != null)
            {
                matching_service_extension.FindHandledInterfaces(all_service_extensions, interfaces, compilation);
                foreach (var result in matching_service_extension.AddedInterfaces)
                {
                    yield return result;
                }
            }
            else
            {
                foreach (var exposed_interface in compilation.FindExposedInterfacesOfServiceExtension(method_name, candidate_namespaces))
                {
                    yield return exposed_interface;
                }
            }
        }
        internal void FindMissingInterfaces()
        {
            MissingInterfaces = RequiredInterfaces.Where(r => !AddedInterfaces.Any(i =>
            i.FullyQualifiedName == r.FullyQualifiedName ||
            (i.FullyQualifiedName == i.GenericTypeName && i.GenericTypeName == r.GenericTypeName)
            )).ToList();
        }
    }
}
