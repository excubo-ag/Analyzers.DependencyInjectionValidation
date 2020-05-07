using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionValidation
{
    internal class ClassOrInterfaceType : Accessible<TypeDeclarationSyntax>
    {
        public string FullyQualifiedName { get; protected set; }
        public string GenericTypeName => FullyQualifiedName == null || !FullyQualifiedName.Contains('<')
            ? FullyQualifiedName
            : FullyQualifiedName.Split('<')[0] + "<" + new string(',', FullyQualifiedName.Count(c => c == ',')) + ">";
        public List<string> TypeArguments => FullyQualifiedName == null || !FullyQualifiedName.Contains('<')
            ? null
            : FullyQualifiedName.Split('<')[1].Split('>')[0].Split(',').ToList();
        public ClassOrInterfaceType(TypeDeclarationSyntax syntax) : base(syntax)
        {
            FullyQualifiedName = syntax.GetFullyQualifiedName();
        }
        public ClassOrInterfaceType(ClassOrInterfaceType second) : base(second as Accessible<TypeDeclarationSyntax>)
        {
            FullyQualifiedName = second.FullyQualifiedName;
        }

        public ClassOrInterfaceType(ITypeSymbol type) : base(is_static: type.IsStatic, type.DeclaredAccessibility)
        {
            var parts = new List<string> { type.Name };
            var ns = type.ContainingNamespace;
            while (ns != null)
            {
                parts.Add(ns.Name);
                ns = ns.ContainingNamespace;
            }
            parts.Reverse();
            parts = parts.Where(p => !string.IsNullOrEmpty(p)).ToList();
            FullyQualifiedName = string.Join(".", parts);
            if (type is INamedTypeSymbol ntype)
            {
                if (ntype.TypeArguments != null && ntype.TypeArguments.Length > 0)
                {
                    FullyQualifiedName = FullyQualifiedName + "<" + string.Join(",", ntype.TypeArguments) + ">";
                }
            }
        }
        public override string ToString() => FullyQualifiedName;

        internal void Specialise(TypeSyntax type, bool even_if_empty = false)
        {
            if (type is GenericNameSyntax gns)
            {
                var type_arguments = gns.TypeArgumentList.Arguments;
                if (type_arguments.Any() && (even_if_empty || !type_arguments.All(t => string.IsNullOrEmpty(t.ToString()))))
                {
                    FullyQualifiedName = FullyQualifiedName.Split('<')[0] + "<" + string.Join(",", type_arguments.Select(t => t.ToString())) + ">";
                }

            }
            else if (type is QualifiedNameSyntax qns)
            {
                Specialise(qns.Right);
            }
        }
        internal void Specialise(MethodDeclarationSyntax declaration)
        {
            if (declaration.TypeParameterList == null)
            {
                return;
            }
            var type_arguments = declaration.TypeParameterList.Parameters;
            if (!type_arguments.Any() || type_arguments.All(t => string.IsNullOrEmpty(t.ToString())))
            {
                return;
            }
            FullyQualifiedName = FullyQualifiedName.Split('<')[0] + "<" + string.Join(",", type_arguments.Select(t => t.ToString())) + ">";
        }
    }
}
