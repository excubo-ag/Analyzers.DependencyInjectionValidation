using DependencyInjectionValidation.Test.Dependency;
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace My.Namespace
{
    public interface ILocalService
    {
    }
    internal class ImplementationOfInterfaceFromOtherAssemblyFullyQualified : DependencyInjectionValidation.Test.Dependency.IOtherAssemblyPublicUse2 // simply implements an interface, no need for service extension
    {
    }
}
