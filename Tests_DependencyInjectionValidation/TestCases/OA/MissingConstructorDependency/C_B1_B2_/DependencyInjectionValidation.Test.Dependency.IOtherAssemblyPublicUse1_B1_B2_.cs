using DependencyInjectionValidation.Test.Dependency;
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace My.Namespace
{
    public interface ILocalService
    {
    }
    internal class C<A1, A2> : ILocalService // requires service extension
    {
        public C(IOtherAssemblyPublicUse1<A1, A2> other_assembly)
        {
        }
    }
    public static class ServiceExtension
    {
        [Exposes(typeof(C<,>)), As(typeof(ILocalService))]
        public static IServiceCollection AddImplementationOfInterfaceFromOtherAssemblyWithDependency<B1, B2>(this IServiceCollection services)
        {
            return services
                .AddScoped<ILocalService, C<B1, B2>>();
        }
    }
}
