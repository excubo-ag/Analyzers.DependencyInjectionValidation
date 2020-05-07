using DependencyInjectionValidation.Test.Dependency;
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionValidation.Test
{
    public interface IInternalDependency { }
    internal class InternalDependency : IInternalDependency
    {
    }
    public class PublicWorker
    {
        public PublicWorker(IOtherAssemblyPublicUse1 external_dependency, IInternalDependency internal_dependency)
        {
        }
    }
    public class Startup
    {
        [DependencyInjectionPoint]
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<IInternalDependency, InternalDependency>();
        }
    }
}
