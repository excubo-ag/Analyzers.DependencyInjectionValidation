using DependencyInjectionValidation.Test.Dependency;
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionValidation.Test
{
    public interface IService { }
    internal class Service : IService { }
    public interface IInternalDependency { }
    // FILLER TO HAVE ERROR ON CORRECT LINE
    internal class InternalDependency : IInternalDependency
    {
        internal InternalDependency(IService service)
        {
        }
    }
    public class PublicWorker
    {
        public PublicWorker(IOtherAssemblyPublicUse1 external_dependency)
        {
        }
    }
    public class Startup
    {
        [DependencyInjectionPoint]
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<IService, Service>()
                .AddOtherAssemblyImplicitUse();
        }
    }
}
