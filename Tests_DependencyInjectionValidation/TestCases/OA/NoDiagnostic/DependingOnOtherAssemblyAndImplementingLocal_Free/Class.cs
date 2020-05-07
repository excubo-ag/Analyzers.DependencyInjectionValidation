using DependencyInjectionValidation.Test.Dependency;
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace My.Namespace
{
    public interface ILocalService
    {
    }
    internal class DependingOnOtherAssemblyAndImplementingLocal<A1, A2> : ILocalService // requires service extension
    {
        public DependingOnOtherAssemblyAndImplementingLocal(IOtherAssemblyPublicUse1<A1, A2> other_assembly)
        {
        }
    }
    public static class ServiceExtension
    {
        [Exposes(typeof(DependingOnOtherAssemblyAndImplementingLocal<,>)), As(typeof(ILocalService))]
        public static IServiceCollection AddImplementationOfInterfaceFromOtherAssemblyWithDependency<B1, B2>(this IServiceCollection services)
        {
            return services
                .AddOtherAssemblyImplicitUseTwoFreeParameters<B1, B2>(parameter)
                .AddScoped<ILocalService, DependingOnOtherAssemblyAndImplementingLocal<B1, B2>>();
        }
    }
}
