using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace DependencyInjectionValidation
{
    internal class Interface : ClassOrInterfaceType
    {
        public Interface(InterfaceDeclarationSyntax syntax) : base(syntax)
        {
        }

        public Interface(ITypeSymbol type) : base(type)
        {
        }

        private Interface(Interface original) : base(original as ClassOrInterfaceType)
        {
        }
        public Interface Clone()
        {
            return new Interface(this);
        }
        internal Interface Instantiate(List<string> selected_type_arguments)
        {
            var copy = this.Clone();
            if (TypeArguments == null || selected_type_arguments == null)
            {
                return copy;
            }
            copy.FullyQualifiedName = copy.FullyQualifiedName.Split('<')[0] + "<" + string.Join(",", selected_type_arguments) + ">";
            return copy;
        }
    }
}