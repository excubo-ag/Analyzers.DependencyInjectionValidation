using DependencyInjectionValidation.Test.Dependency;
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace My.Namespace
{
    public interface ILocalService
    {
    }
    internal class C : ILocalService // requires service extension
    {
        public C(IOtherAssemblyPublicUse1 other_assembly)
        {
        }
    }
    public static class ServiceExtension
    {
        [Exposes(typeof(C)), As(typeof(ILocalService))]
        public static IServiceCollection AddImplementationOfInterfaceFromOtherAssemblyWithDependency(this IServiceCollection services)
        {
            return services
                .AddScoped<ILocalService, C>();
        }
    }
}
