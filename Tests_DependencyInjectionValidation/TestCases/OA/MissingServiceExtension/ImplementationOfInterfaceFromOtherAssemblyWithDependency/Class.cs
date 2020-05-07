using DependencyInjectionValidation.Test.Dependency;
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace My.Namespace
{
    public interface ILocalService
    {
    }
    internal class ImplementationOfInterfaceFromOtherAssemblyWithDependency : IOtherAssemblyPublicUse1<string> // requires service extension for ILocalService
    {
        public ImplementationOfInterfaceFromOtherAssemblyWithDependency(ILocalService dependency)
        {

        }
    }
}
