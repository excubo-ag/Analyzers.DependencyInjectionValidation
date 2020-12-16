using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DependencyInjectionValidation
{
    public static class SemanticExtensions
    {
        public static ITypeSymbol GetTypeInfo(this ISymbol symbol)
        {

            if (symbol is IPropertySymbol property)
            {
                return property.GetMethod.ReturnType;
            }
            else if (symbol is IParameterSymbol parameter)
            {
                return parameter.Type;
            }
            else if (symbol is IFieldSymbol field)
            {
                return field.Type;
            }
            else if (symbol is ILocalSymbol local)
            {
                return local.Type;
            }
            else if (symbol is IMethodSymbol method)
            {
                return method.ReturnType;
            }
            else
            {
                return null;
            }
        }
    }
    public static class SyntaxExtensions
    {
        public static string GetNameWithEmptyTypeParameters(this TypeSyntax name_syntax)
        {
            switch (name_syntax)
            {
                case GenericNameSyntax gns:
                    return gns.Identifier.ToString() + "<" + new string(',', gns.TypeArgumentList.Arguments.Count - 1) + ">";
                case SimpleNameSyntax sns:
                    return sns.ToString();
                case QualifiedNameSyntax qns:
                    return qns.Left.ToString() + "." + qns.Right.GetNameWithEmptyTypeParameters();
                case PredefinedTypeSyntax pts:
                    return pts.ToString();
                case TupleTypeSyntax tts:
                    return tts.ToString();
                default:
                    throw new NotSupportedException();
            }
        }
        public static int GetGenericTypeArgumentCount(this TypeSyntax name_syntax)
        {
            switch (name_syntax)
            {
                case GenericNameSyntax gns:
                    return gns.TypeArgumentList.Arguments.Count;
                case SimpleNameSyntax _:
                    return 0;
                case QualifiedNameSyntax qns:
                    return qns.Right.GetGenericTypeArgumentCount();
                case PredefinedTypeSyntax _:
                    return 0;
                case TupleTypeSyntax tts:
                    return 0;
                default:
                    throw new NotSupportedException();
            }
        }
        public static List<string> GetCandidateNamespaces(this TypeSyntax type_syntax)
        {
            var containing_namespace = type_syntax.FindParentNamespace();
            var using_directives = type_syntax.SyntaxTree.GetRoot().FindUsings();
            return using_directives.Select(ud => ud.Name)
                .Append(containing_namespace.Name)
                .Where(n => n != null)
                .Select(n => n.ToString())
                .Append(string.Empty)
                .ToList();
        }
        public static IEnumerable<MethodDeclarationSyntax> FindMethods(this ClassDeclarationSyntax class_declaration)
        {
            foreach (var child_node in class_declaration.ChildNodes())
            {
                if (child_node is MethodDeclarationSyntax method_node)
                {
                    yield return method_node;
                }
            }
        }
        public static IEnumerable<UsingDirectiveSyntax> FindUsings(this SyntaxNode node)
        {
            foreach (var child_node in node.ChildNodes())
            {
                if (child_node is UsingDirectiveSyntax using_directive)
                {
                    yield return using_directive;
                }
                if (child_node is NamespaceDeclarationSyntax namespace_declaration_syntax)
                {
                    foreach (var result in FindUsings(namespace_declaration_syntax))
                    {
                        yield return result;
                    }
                }
            }
        }
        internal static IEnumerable<Class> FindClasses(this SyntaxNode node)
        {
            foreach (var child_node in node.ChildNodes())
            {
                if (child_node is NamespaceDeclarationSyntax namespace_node)
                {
                    foreach (var result in namespace_node.FindClasses())
                    {
                        yield return result;
                    }
                }
                else if (child_node is ClassDeclarationSyntax class_node)
                {
                    yield return new Class(class_node);
                }
            }
        }
        internal static IEnumerable<Interface> FindInterfaces(this SyntaxNode node)
        {
            foreach (var child_node in node.ChildNodes())
            {
                if (child_node is NamespaceDeclarationSyntax namespace_node)
                {
                    foreach (var result in namespace_node.FindInterfaces())
                    {
                        yield return result;
                    }
                }
                else if (child_node is InterfaceDeclarationSyntax interface_node)
                {
                    yield return new Interface(interface_node);
                }
            }
        }
        internal static string GetFullyQualifiedName<TSyntax>(this TSyntax syntax) where TSyntax : TypeDeclarationSyntax
        {
            var containing_namespace = syntax.FindParentNamespace();
            var @namespace = containing_namespace?.Name?.ToString();
            var own_name = syntax.Identifier.ToString();
            var generic_arguments = syntax.TypeParameterList?.Parameters.Count ?? 0;
            var ns_prefix = string.IsNullOrEmpty(@namespace) ? string.Empty : @namespace + ".";
            var generics_suffix = generic_arguments == 0 ? string.Empty : "<" + string.Join(",", syntax.TypeParameterList?.Parameters.Select(p => p.ToString()) ?? Enumerable.Empty<string>()) + ">";
            var fully_qualified_name = ns_prefix + own_name + generics_suffix;
            return fully_qualified_name;
        }
        public static NamespaceDeclarationSyntax FindParentNamespace(this SyntaxNode node) => FindParentOfType<NamespaceDeclarationSyntax>(node);
        [DebuggerStepThrough]
        public static TSyntax FindParentOfType<TSyntax>(this SyntaxNode node) where TSyntax : SyntaxNode
        {
            node = node?.Parent;
            while (node != null && !(node is TSyntax))
            {
                node = node.Parent;
            }
            return node is TSyntax nds ? nds : null;
        }
        public static IEnumerable<MemberAccessExpressionSyntax> FindMemberAccessExpressions(this SyntaxNode source_node)
        {
            var potential_nodes = new List<SyntaxNode>
            {
                source_node
            };
            while (potential_nodes.Any())
            {
                var current_node = potential_nodes.Last();
                potential_nodes.RemoveAt(potential_nodes.Count - 1);
                if (current_node is MemberAccessExpressionSyntax value)
                {
                    yield return value;
                    potential_nodes.AddRange(current_node.ChildNodes().OfType<InvocationExpressionSyntax>().SelectMany(i => i.ChildNodes()));
                }
                else if (current_node is IdentifierNameSyntax
                      || current_node is GenericNameSyntax
                      || current_node is TypeArgumentListSyntax
                      || current_node is ArgumentListSyntax)
                {
                    continue;
                    // there cannot be a relevant invocation expression in the child tree here
                }
                else
                {
                    potential_nodes.AddRange(current_node.ChildNodes());
                }

            }
        }
    }
}