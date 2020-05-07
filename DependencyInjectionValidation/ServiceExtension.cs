using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionValidation
{
    internal class ServiceExtension : ServiceExtensionBase
    {
        public List<Class> HandledTypes { get; } = new List<Class>();
        public ServiceExtension(string class_fully_qualified_name, MethodDeclarationSyntax declaration)
        {
            var generic_suffix = declaration.TypeParameterList != null ? "<" + string.Join(",", declaration.TypeParameterList.Parameters.Select(p => p.ToString())) + ">" : "";
            FullyQualifiedName = class_fully_qualified_name + "." + declaration.Identifier.ToString() + generic_suffix;
            Declaration = declaration;
        }
    }
}
