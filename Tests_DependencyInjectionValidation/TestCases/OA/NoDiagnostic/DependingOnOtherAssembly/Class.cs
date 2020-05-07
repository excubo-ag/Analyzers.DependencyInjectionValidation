using DependencyInjectionValidation.Test.Dependency;
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace My.Namespace
{
    public interface ILocalService
    {
    }
    internal class DependingOnOtherAssembly //requires startup to add services IOtherAssemblyPublicUse1 and ILocalService, but no service extension for this
    {
        public DependingOnOtherAssembly(IOtherAssemblyPublicUse1 other_assembly, ILocalService local)
        {
        }
    }
}
