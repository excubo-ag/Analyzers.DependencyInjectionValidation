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
    public class DependingOnServiceTInInternalConstructor : IPublicUse1 // cannot be constructed freely, needs service extension
    {
        internal DependingOnServiceTInInternalConstructor(IPublicUse2<object> internal_service)
        {

        }
    }
    public static class ServiceExtension
    {
        [Exposes(typeof(DependingOnServiceTInInternalConstructor)), As(typeof(IPublicUse1))]
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            return services
                .AddSingleton<IPublicUse2<object>, Irrelevant>()
                .AddScoped<IPublicUse1, DependingOnServiceTInInternalConstructor>();
        }
    }
}
