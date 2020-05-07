using DependencyInjectionValidation.Test.Dependency;
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Types
{
    #region public interfaces
    public interface IPublicUse1
    {
    }
    public interface IPublicUse1<T>
    {
    }
    public interface IPublicUse1<T1, T2>
    {
    }
    public interface IPublicUse2
    {
    }
    public interface IPublicUse2<T>
    {
    }
    public interface IPublicUse2<T1, T2>
    {
    }
    #endregion
    #region internal interfaces
    internal interface IInternalUse
    {
    }
    internal interface IInternalUse<T>
    {
    }
    internal interface IInternalUse<T1, T2>
    {
    }
    #endregion
    public class ImplicitUse : JustAClass
    {
        public ImplicitUse(IPublicUse1 dependency)
        {

        }
    }
    public static class ServiceExtension
    {
        [Exposes(typeof(ImplicitUse))]
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            return services
                .AddSingleton<IPublicUse1, Irrelevant>();
        }
    }
}
