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
        public static IServiceCollection AddService1(this IServiceCollection services)
        {
            return services
                .AddSingleton<ILocalService, Irrelevant>()
                .AddScoped<IOtherAssemblyPublicUse1<string>, ImplementationOfInterfaceFromOtherAssemblyWithDependency>();
        }
        [Exposes(typeof(ImplementationOfInterfaceFromOtherAssemblyWithDependency)), As(typeof(IOtherAssemblyPublicUse1<string>))]
        public static IServiceCollection AddService2(this IServiceCollection services)
        {
            return services
                .AddSingleton<ILocalService, Irrelevant>()
                .AddScoped<IOtherAssemblyPublicUse1<string>, ImplementationOfInterfaceFromOtherAssemblyWithDependency>();
        }
    }
}
