using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionValidation
{
    internal class ServiceExtensionBase
    {
        public string FullyQualifiedName { get; protected set; }
        public string GenericTypeName => FullyQualifiedName == null || !FullyQualifiedName.Contains('<')
            ? FullyQualifiedName
            : FullyQualifiedName.Split('<')[0] + "<" + new string(',', FullyQualifiedName.Count(c => c == ',')) + ">";
        public List<string> TypeArguments => FullyQualifiedName == null || !FullyQualifiedName.Contains('<')
            ? null
            : FullyQualifiedName.Split('<')[1].Split('>')[0].Split(',').ToList();
        public MethodDeclarationSyntax Declaration { get; protected set; }
        protected IEnumerable<Interface> FindExplicitelyIgnoredInterfaces(List<Interface> interfaces, Compilation compilation)
        {
            return FindInterfacesInAttributes(interfaces, compilation, "IgnoreDependency");
        }
        protected IEnumerable<Interface> FindExplicitelyInjectedInterfaces(List<Interface> interfaces, Compilation compilation)
        {
            return FindInterfacesInAttributes(interfaces, compilation, "Injects");
        }
        private IEnumerable<Interface> FindInterfacesInAttributes(List<Interface> interfaces, Compilation compilation, string attribute_name)
        {
            return Declaration.AttributeLists
                .SelectMany(a => a.Attributes) // find all attributes
                .Where(attr => attr.Name.ToString() == attribute_name) // that are the injects attribute
                .SelectMany(attr => attr.ArgumentList.Arguments) // get their arguments
                .Select(a => a.Expression) // and their expressions
                .OfType<TypeOfExpressionSyntax>() // that are typeof() expressions
                .Select(parameter => FindAndSpecialise(parameter, interfaces, compilation))
                .Where(i => i != null);
        }
        private Interface FindAndSpecialise(TypeOfExpressionSyntax parameter, List<Interface> interfaces, Compilation compilation)
        {
            var type = Helpers.FindInterface(parameter.Type, interfaces, compilation);
            type?.Specialise(parameter.Type, even_if_empty: true);
            return type;
        }
        protected Interface GetInterfaceFromStandardExtensionMethod(MemberAccessExpressionSyntax member_access_expression, List<Interface> interfaces, Compilation compilation)
        {
            TypeSyntax service_type_argument;
            if (member_access_expression.Name is GenericNameSyntax gns)
            {
                service_type_argument = gns.TypeArgumentList.Arguments.First();
            }
            else
            {
                service_type_argument = ((member_access_expression.Parent as InvocationExpressionSyntax).ArgumentList.Arguments.First().Expression as TypeOfExpressionSyntax)?.Type;
            }
            if (service_type_argument == null)
            {
                var first_argument = (member_access_expression.Parent as InvocationExpressionSyntax).ArgumentList.Arguments.FirstOrDefault();
                var model = compilation.GetSemanticModel(first_argument.SyntaxTree);
                var info = model.GetSymbolInfo((first_argument.Expression as IdentifierNameSyntax));
                if (info.Symbol != null)
                {
                    var symbol_type = info.Symbol.GetTypeInfo();
                    if (symbol_type != default)
                    {
                        return new Interface(symbol_type);
                    }
                }
                return null;
            }
            return Helpers.FindInterface(service_type_argument, interfaces, compilation);
        }
    }
}