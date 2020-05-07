using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionValidation
{
    internal static class Helpers
    {
        internal static Class FindClass(TypeSyntax type, List<Class> known_classes, Compilation compilation)
        {
            var n_generic_type_arguments = type.GetGenericTypeArgumentCount();
            var own_name = type.GetNameWithEmptyTypeParameters();
            var candidate_namespaces = type.GetCandidateNamespaces();
            var typename_parts = own_name.Split('.');
            if (typename_parts.Length > 1)
            {
                own_name = typename_parts.Last();
                var qualified_typename_part = string.Join(".", typename_parts.Take(typename_parts.Length - 1));
                candidate_namespaces = new List<string> { qualified_typename_part };
            }
            var match = FindClass(own_name, n_generic_type_arguments, candidate_namespaces, compilation, known_classes);
            match?.Specialise(type);
            return match;
        }
        internal static Interface FindInterface(TypeSyntax type, List<Interface> known_interfaces, Compilation compilation)
        {
            var n_generic_type_arguments = type.GetGenericTypeArgumentCount();
            var own_name = type.GetNameWithEmptyTypeParameters();
            var candidate_namespaces = type.GetCandidateNamespaces();
            var typename_parts = own_name.Split('.');
            if (typename_parts.Length > 1)
            {
                own_name = typename_parts.Last();
                var qualified_typename_part = string.Join(".", typename_parts.Take(typename_parts.Length - 1));
                candidate_namespaces = new List<string> { qualified_typename_part };
            }
            var match = FindInterface(own_name, n_generic_type_arguments, candidate_namespaces, compilation, known_interfaces);
            match?.Specialise(type);
            return match;
        }
        internal static Interface FindInterface(string typename, List<Interface> known_interfaces, Compilation compilation)
        {
            if (typename.Contains("<"))
            {
                throw new NotSupportedException("Generics are not supported here yet");
            }
            var typename_parts = typename.Split('.');
            typename = typename_parts.Last();
            var n_generic_type_arguments = 0;
            var candidate_namespaces = new List<string> { string.Join(".", typename_parts.Take(typename_parts.Length - 1)) };
            return FindInterface(typename, n_generic_type_arguments, candidate_namespaces, compilation, known_interfaces);
        }
        private static Class FindClass(string typename, int n_generic_type_arguments, List<string> candidate_namespaces, Compilation compilation, List<Class> classes)
        {
            var match = classes.FirstOrDefault(i => candidate_namespaces.Any(cn => i.GenericTypeName == $"{cn}.{typename}"));
            if (match != default)
            {
                return match.Clone();
            }
            var result = compilation.FindInAssemblies(typename, n_generic_type_arguments, TypeKind.Class);
            return result == null ? default : new Class(result);
        }
        private static Interface FindInterface(string typename, int n_generic_type_arguments, List<string> candidate_namespaces, Compilation compilation, List<Interface> interfaces)
        {
            var match = interfaces.FirstOrDefault(i => candidate_namespaces.Any(cn => i.GenericTypeName == $"{cn}.{typename}"));
            if (match != default)
            {
                return match.Clone();
            }
            var result = compilation.FindInAssemblies(typename, n_generic_type_arguments, TypeKind.Interface);
            return result == null ? default : new Interface(result);
        }
    }
}
