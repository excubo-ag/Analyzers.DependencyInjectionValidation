using DependencyInjectionValidation.Test.Dependency;
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace My.Namespace
{
    public interface ILocalService
    {
    }
    internal class DependingOnOtherAssemblyAndImplementingLocal : ILocalService // requires service extension
    {
        public DependingOnOtherAssemblyAndImplementingLocal(IOtherAssemblyPublicUse1 other_assembly)
        {
        }
    }
}
