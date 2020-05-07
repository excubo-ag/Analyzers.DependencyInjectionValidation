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
    public class DependingOnServiceInPublicConstructor<U1, U2> : IPublicUse1<U1> // cannot be constructed freely, needs service extension
    {
        public DependingOnServiceInPublicConstructor()
        {

        }
        [Inject]
        public IPublicUse2<U2> Property { get; set; }
    }
    public static class ServiceExtension
    {
        [Exposes(typeof(DependingOnServiceInPublicConstructor<object, string>)), As(typeof(IPublicUse1<object>))]
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            return services
                .AddScoped<IPublicUse1<object>, DependingOnServiceInPublicConstructor<object, string>>();
        }
    }
}
