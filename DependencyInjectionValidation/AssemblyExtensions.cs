using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DependencyInjectionValidation
{
    internal static class AssemblyExtensions
    {
        internal static IEnumerable<Interface> FindExposedInterfacesOfServiceExtension(this Compilation compilation, string method_name, List<string> candidate_namespaces)
        {
            var method_match = compilation.FindServiceExtensionMethod(method_name, candidate_namespaces);
            if (method_match != null)
            {
                return method_match
                    .GetAttributes()
                    .Where(a => a.AttributeClass.Name == "AsAttribute")
                    .Select(a => a.ConstructorArguments.First())
                    .Select(tc => tc.Value as INamedTypeSymbol)
                    .Select(ts => new Interface(ts));
            }
            return Enumerable.Empty<Interface>();
        }
        private static IMethodSymbol FindServiceExtensionMethod(this Compilation compilation, string method_name, List<string> candidate_namespaces)
        {
            foreach (var assembly_symbol in compilation.SourceModule.ReferencedAssemblySymbols)
            {
                var match = assembly_symbol.GlobalNamespace.FindServiceExtensionMethod(method_name, candidate_namespaces);
                if (match != null)
                {
                    return match;
                }
            }
            return null;
        }
        private static IMethodSymbol FindServiceExtensionMethod(this INamespaceSymbol namespace_symbol, string method_name, List<string> candidate_namespaces)
        {
            if (candidate_namespaces.Any(c => c == string.Empty))
            {
                // we are in the position where the searched method name could be in the currently selected namespace.
                var match = namespace_symbol.FindServiceExtensionMethodByName(method_name);
                if (match != null)
                {
                    return match;
                }
            }
            foreach (var sub_namespace in namespace_symbol.GetNamespaceMembers())
            {
                var candidate_sub_namespaces = candidate_namespaces.Where(cn => cn.StartsWith(sub_namespace.Name)).Select(cn => cn.Substring(sub_namespace.Name.Length)).Select(cn => cn.StartsWith(".") ? cn.Substring(1) : cn).ToList();
                if (candidate_sub_namespaces.Any())
                {
                    var match = sub_namespace.FindServiceExtensionMethod(method_name, candidate_sub_namespaces);
                    if (match != null)
                    {
                        return match;
                    }
                }
            }
            return null;
        }
        private static IMethodSymbol FindServiceExtensionMethodByName(this INamespaceSymbol namespace_symbol, string method_name)
        {
            return namespace_symbol
                .GetTypeMembers() // look for classes
                .Where(t => t.IsStatic && t.MemberNames.Contains(method_name)) // that are static and contain the method_name as member
                .SelectMany(t => t
                    .GetMembers(method_name) // then look at all matching members
                    .OfType<IMethodSymbol>() // that are methods
                    .Where(m => m.GetAttributes().Where(a => a.AttributeClass.Name == "AsAttribute").Any())) // and check if any has the "As" attribute
                .FirstOrDefault();
        }
        internal static INamedTypeSymbol FindInAssemblies(this Compilation compilation, string typename, int n_generic_type_arguments, TypeKind type_kind)
        {
            typename = typename.Split('<')[0];
            foreach (var assembly_symbol in compilation.SourceModule.ReferencedAssemblySymbols)
            {
                var match = assembly_symbol.GlobalNamespace.FindInNamespaceRecursively(typename, n_generic_type_arguments, type_kind);
                if (match != null)
                {
                    return match;
                }
            }
            return default;
        }
        internal static INamedTypeSymbol FindInNamespaceRecursively(this INamespaceSymbol namespace_symbol, string typename, int n_generic_type_arguments, TypeKind type_kind)
        {
            var ns_candidates = new List<INamespaceSymbol> { namespace_symbol };
            while (ns_candidates.Any())
            {
                var current = ns_candidates.Last();
                ns_candidates.RemoveAt(ns_candidates.Count - 1);
                ns_candidates.AddRange(current.GetNamespaceMembers());
                foreach (var type in current.GetTypeMembers().Where(t => t.TypeKind == type_kind))
                {
                    if (type.Name.StartsWith(typename) && ((!type.IsGenericType && n_generic_type_arguments == 0) || (type.IsGenericType && type.TypeArguments.Count() == n_generic_type_arguments)))
                    {
                        return type;
                    }
                }
            }
            return null;

        }
    }
}
