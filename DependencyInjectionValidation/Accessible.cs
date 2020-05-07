using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace DependencyInjectionValidation
{
    internal class Accessible<TSyntax> where TSyntax : MemberDeclarationSyntax
    {
        private readonly Accessibility accessibility;
        public bool IsStatic { get; }
        public bool IsPublic => accessibility == Accessibility.Public;
        public bool IsInternal => accessibility == Accessibility.Internal || accessibility == Accessibility.ProtectedAndInternal;
        public bool IsProtected => accessibility == Accessibility.Protected || accessibility == Accessibility.ProtectedAndInternal;
        public bool IsPrivate => accessibility == Accessibility.Private;
        public TSyntax Declaration { get; }
        public Accessible(TSyntax syntax)
        {
            Declaration = syntax;
            IsStatic = syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
            if (syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
            {
                accessibility = Accessibility.Public;
            }
            else if (syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword)))
            {
                accessibility = Accessibility.Internal;
            }
            else if (syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword)))
            {
                accessibility = Accessibility.Protected;
            }
            else
            {
                accessibility = Accessibility.Private;
            }
        }
        public Accessible(Accessible<TSyntax> second)
        {
            (Declaration, IsStatic, accessibility) = (second.Declaration, second.IsStatic, second.accessibility);
        }
        protected Accessible(bool is_static, Accessibility accessibility)
        {
            IsStatic = is_static;
            this.accessibility = accessibility;
        }
    }
}
