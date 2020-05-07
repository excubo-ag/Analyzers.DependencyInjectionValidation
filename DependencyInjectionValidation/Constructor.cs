using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionValidation
{
    internal class Constructor : Accessible<ConstructorDeclarationSyntax>
    {
        public List<Interface> Dependencies { get; private set; } = new List<Interface>();
        public Constructor(ConstructorDeclarationSyntax syntax) : base(syntax)
        {
        }
        private Constructor() : base(is_static: false, Accessibility.Public)
        {

        }
        private Constructor(Constructor second) : base(second as Accessible<ConstructorDeclarationSyntax>)
        {
            Dependencies = second.Dependencies.Select(d => d.Clone()).ToList();
        }
        internal static Constructor NewDefaultConstructor()
        {
            return new Constructor();
        }
        internal Constructor Clone()
        {
            return new Constructor(this);
        }
        internal Constructor Instantiate(Dictionary<string, string> mapping)
        {
            var copy = this.Clone();
            copy.Dependencies = copy.Dependencies.Select(d =>
            {
                if (d.TypeArguments == null)
                {
                    return d;
                }
                var relevant_selected_type_arguments = d.TypeArguments.Select(source => mapping.ContainsKey(source) ? mapping[source] : source).ToList();
                return d.Instantiate(relevant_selected_type_arguments);
            }).ToList();
            return copy;
        }
        public void FindDependencies(List<Interface> known_interfaces, Compilation compilation)
        {
            if (Declaration == null || Declaration.ParameterList == null || !Declaration.ParameterList.Parameters.Any())
            {
                return;
            }
            foreach (var parameter_type in Declaration.ParameterList.Parameters.Select(p => p.Type))
            {
                var match = Helpers.FindInterface(parameter_type, known_interfaces, compilation);
                if (match != null)
                {
                    Dependencies.Add(match);
                }
            }
        }
    }
}
