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
    public static class ServiceExtension
    {
        [Exposes(typeof(ImplementationOfInterfaceFromOtherAssemblyWithDependency)), As(typeof(IOtherAssemblyPublicUse1<string>))]
        public static IServiceCollection AddImplementationOfInterfaceFromOtherAssemblyWithDependency(this IServiceCollection services)
        {
            return services
                .AddScoped<IOtherAssemblyPublicUse1<string>, ImplementationOfInterfaceFromOtherAssemblyWithDependency>();
        }
    }
}
