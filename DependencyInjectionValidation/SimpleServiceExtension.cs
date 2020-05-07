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
        public List<Interface> AddedInterfaces { get; private set; }
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
            var added_interfaces = new List<Interface>
            {
                Helpers.FindInterface("System.IServiceProvider", interfaces, compilation) // this type is always available for DI
            };
            if (include_ignored)
            {
                added_interfaces.AddRange(FindExplicitelyIgnoredInterfaces(interfaces, compilation));
            }
            added_interfaces.AddRange(FindAddedInterfaces(all_service_extensions, interfaces, compilation));
            AddedInterfaces = added_interfaces;
        }
        private IEnumerable<Interface> FindExplicitelyIgnoredInterfaces(List<Interface> interfaces, Compilation compilation)
        {
            return Declaration.AttributeLists
                .SelectMany(a => a.Attributes) // find all attributes
                .Where(attr => attr.Name.ToString() == "IgnoreDependency") // that are the ignore attribute
                .SelectMany(attr => attr.ArgumentList.Arguments) // get their arguments
                .Select(a => a.Expression) // and their expressions
                .OfType<TypeOfExpressionSyntax>() // that are typeof() expressions
                .Select(parameter => FindExplicitelyIgnoredInterfaces(parameter, interfaces, compilation))
                .Where(i => i != null);
        }
        private Interface FindExplicitelyIgnoredInterfaces(TypeOfExpressionSyntax parameter, List<Interface> interfaces, Compilation compilation)
        {
            var type = Helpers.FindInterface(parameter.Type, interfaces, compilation);
            if (type != null)
            {
                type.Specialise(parameter.Type, even_if_empty: true);
                return type;
            }
            return null;
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
