using DependencyInjectionValidation.Test.Dependency;
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace My.Namespace
{
    public interface ILocalService
    {
    }
    internal class Impl : IOtherAssemblyPublicUse1<string> // requires service extension for ILocalService
    {
        public Impl(ILocalService dependency)
        {
        }
    }
    public static class ServiceExtension
    {
        [Exposes(typeof(Impl)), As(typeof(IOtherAssemblyPublicUse1<string>))]
        public static IServiceCollection AddImplementationOfInterfaceFromOtherAssemblyWithDependency(this IServiceCollection services)
        {
            return services
                .AddSingleton<ILocalService, Irrelevant>();
        }
    }
}
