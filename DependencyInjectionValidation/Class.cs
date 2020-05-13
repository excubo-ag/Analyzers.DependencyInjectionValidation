using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionValidation
{
    internal class Class : ClassOrInterfaceType
    {
        public Class(ClassDeclarationSyntax syntax) : base(syntax)
        {
            Constructors = syntax.Members.OfType<ConstructorDeclarationSyntax>().Select(s => new Constructor(s)).ToList();
            if (!Constructors.Any())
            {
                Constructors.Add(Constructor.NewDefaultConstructor());
            }
        }
        private Class(Class original) : base(original as ClassOrInterfaceType)
        {
            Bases = original.Bases.Select(b => b.Clone()).ToList();
            Constructors = original.Constructors.Select(c => c.Clone()).ToList();
            InjectedPropertyTypes = original.InjectedPropertyTypes.Select(i => i.Clone()).ToList();
        }
        public Class(INamedTypeSymbol type) : base(type)
        {
            // this class is in another assembly. we do not need base classes of constructors here, as that's only used to diagnose issues in the current assembly.
        }
        public Class Clone()
        {
            return new Class(this);
        }

        internal IEnumerable<Interface> RequiredInterfaces()
        {
            return Enumerable.Empty<Interface>()
                .Concat(Bases.Where(b => b.IsPublic))
                .Concat(Constructors.SelectMany(constructor => constructor.Dependencies))
                .Concat(InjectedPropertyTypes);
        }
        public Class Instantiate(List<string> selected_type_arguments)
        {
            var copy = this.Clone();
            if (TypeArguments == null || selected_type_arguments == null)
            {
                return copy;
            }
            if (!TypeArguments.All(a => a == string.Empty))
            {
                var mapping = new Dictionary<string, string>();
                foreach (var (source, target) in TypeArguments.Zip(selected_type_arguments, (s, t) => (s, t)))
                {
                    mapping.Add(source, target);
                }
                copy.Bases = copy.Bases.Select(b =>
                {
                    if (b.TypeArguments == null)
                    {
                        return b;
                    }
                    var relevant_selected_type_arguments = b.TypeArguments.Select(source => mapping.ContainsKey(source) ? mapping[source] : source).ToList();
                    return b.Instantiate(relevant_selected_type_arguments);
                }).ToList();
                copy.Constructors = copy.Constructors.Select(c => c.Instantiate(mapping)).ToList();
                copy.InjectedPropertyTypes = copy.InjectedPropertyTypes.Select(b =>
                {
                    if (b.TypeArguments == null)
                    {
                        return b;
                    }
                    var relevant_selected_type_arguments = b.TypeArguments.Select(source => mapping.ContainsKey(source) ? mapping[source] : source).ToList();
                    return b.Instantiate(relevant_selected_type_arguments);
                }).ToList();
            }
            copy.FullyQualifiedName = copy.FullyQualifiedName.Split('<')[0] + "<" + string.Join(",", selected_type_arguments) + ">";
            return copy;
        }
        public void FindBases(List<Interface> known_interfaces, Compilation compilation)
        {
            if (Declaration.BaseList == null)
            {
                return;
            }
            foreach (var type in Declaration.BaseList.Types.Select(bt => bt.Type))
            {
                var match = Helpers.FindInterface(type, known_interfaces, compilation);
                if (match != null)
                {
                    Bases.Add(match);
                }
            }
        }
        public void FindProperties(List<Interface> known_interfaces, Compilation compilation)
        {
            foreach (var property in Declaration.Members.OfType<PropertyDeclarationSyntax>())
            {
                var has_inject_attribute = property.AttributeLists.SelectMany(a => a.Attributes).Any(a =>
                {
                    switch (a.Name.ToString())
                    {
                        case "Inject":
                        case "InjectAttribute":
                        case "global::Microsoft.AspNetCore.Components.InjectAttribute":
                            return true;
                        default:
                            return false;
                    }
                });
                if (!has_inject_attribute)
                {
                    continue;
                }
                var match = Helpers.FindInterface(property.Type, known_interfaces, compilation);
                if (match != null)
                {
                    InjectedPropertyTypes.Add(match);
                }
            }
        }
        public List<Interface> Bases { get; private set; } = new List<Interface>();
        public List<Constructor> Constructors { get; private set; }
        public List<Interface> InjectedPropertyTypes { get; private set;  } = new List<Interface>();
    }
    internal static class ClassExtensions
    {
        internal static bool RequiresServiceExtension(this Class @class)
        {
            if (@class.IsPublic)
            {
                if (@class.Constructors.Any(constructor => constructor.IsPublic && constructor.Dependencies.Count == 0)
                 && !@class.InjectedPropertyTypes.Any())
                {
                    return false; // the class is trivially constructible, so we assume it's left for the user to do.
                }
                if (@class.Constructors.All(constructor => constructor.IsPrivate || constructor.IsProtected))
                {
                    return false; // the class is not constructible by the DI system.
                }
            }
            else if (@class.IsPrivate)
            {
                return false; // the class is not constructible by the DI system.
            }
            else if (@class.IsProtected)
            {
                return false; // the class is not constructible by the DI system.
            }
            else if (@class.IsInternal)
            {
                if (@class.Bases.All(b => !b.IsPublic)) // this class seems to be for internal use only.
                {
                    return false;
                }
                if (@class.Constructors.Any(constructor => (constructor.IsPublic || constructor.IsInternal) && constructor.Dependencies.Count == 0)
                 && !@class.InjectedPropertyTypes.Any())
                {
                    return false;
                }
            }
            return true;
        }
        internal static bool RequiresDependencyInjection(this Class @class)
        {
            if (@class.IsPublic)
            {
                if (@class.Constructors.Any(constructor => constructor.IsPublic && constructor.Dependencies.Count == 0)
                 && !@class.InjectedPropertyTypes.Any())
                {
                    return false; // the class is trivially constructible, so we assume it's left for the user to do.
                }
                if (@class.Constructors.All(constructor => constructor.IsPrivate || constructor.IsProtected))
                {
                    return false; // the class is not constructible by the DI system.
                }
            }
            else if (@class.IsPrivate)
            {
                return false; // the class is not constructible by the DI system.
            }
            else if (@class.IsProtected)
            {
                return false; // the class is not constructible by the DI system.
            }
            else if (@class.IsInternal)
            {
                if (@class.Constructors.Any(constructor => (constructor.IsPublic || constructor.IsInternal) && constructor.Dependencies.Count == 0)
                 && !@class.InjectedPropertyTypes.Any())
                {
                    return false; // the class is trivially constructible, so we assume it's left for the user to do.
                }
            }
            return true;
        }
        internal static bool HasServiceExtension(this Class @class, List<ServiceExtension> di_extension_methods)
        {
            return di_extension_methods.Any(em => em.HandledTypes.Any(ht => ht.GenericTypeName == @class.GenericTypeName));
        }
    }
}
