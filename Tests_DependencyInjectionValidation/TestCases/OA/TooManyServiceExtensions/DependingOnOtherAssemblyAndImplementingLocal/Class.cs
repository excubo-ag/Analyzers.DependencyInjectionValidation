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
    public static class ServiceExtension
    {
        [Exposes(typeof(DependingOnOtherAssemblyAndImplementingLocal)), As(typeof(ILocalService))]
        public static IServiceCollection AddService1(this IServiceCollection services)
        {
            return services
                .AddOtherAssemblyDependingOnServiceInPublicConstructor()
                .AddScoped<ILocalService, DependingOnOtherAssemblyAndImplementingLocal>();
        }
        [Exposes(typeof(DependingOnOtherAssemblyAndImplementingLocal)), As(typeof(ILocalService))]
        public static IServiceCollection AddService2(this IServiceCollection services)
        {
            return services
                .AddOtherAssemblyDependingOnServiceInPublicConstructor()
                .AddScoped<ILocalService, DependingOnOtherAssemblyAndImplementingLocal>();
        }
    }
}
