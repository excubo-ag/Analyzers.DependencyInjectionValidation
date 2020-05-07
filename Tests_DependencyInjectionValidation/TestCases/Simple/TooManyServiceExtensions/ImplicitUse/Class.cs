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
    internal class ImplicitUse : IPublicUse1 // must be exposed through service extension
    {
    }
    public static class ServiceExtension
    {
        [Exposes(typeof(ImplicitUse)), As(typeof(IPublicUse1))]
        public static IServiceCollection AddService1(this IServiceCollection services)
        {
            return services;
        }
        [Exposes(typeof(ImplicitUse)), As(typeof(IPublicUse1))]
        public static IServiceCollection AddService2(this IServiceCollection services)
        {
            return services;
        }
    }
}
