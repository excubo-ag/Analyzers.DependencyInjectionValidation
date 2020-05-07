using DependencyInjectionValidation.Test.Dependency;
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace My.Namespace
{
    public interface ILocalService
    {
    }
    internal class ImplementationOfInterfaceFromOtherAssembly : IOtherAssemblyPublicUse1 // simply implements an interface, no need for service extension
    {
    }
}
