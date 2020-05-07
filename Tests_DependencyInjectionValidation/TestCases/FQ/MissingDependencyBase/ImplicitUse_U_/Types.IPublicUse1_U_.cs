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
    internal class ImplicitUse<U> : IPublicUse1<U> // must be exposed through service extension
    {
    }
    // FILLER TO HAVE THE ERROR ON THE CORRECT LINE
    // FILLER TO HAVE THE ERROR ON THE CORRECT LINE
    // FILLER TO HAVE THE ERROR ON THE CORRECT LINE
    // FILLER TO HAVE THE ERROR ON THE CORRECT LINE
    public static class ServiceExtension
    {
        [Exposes(typeof(Types.ImplicitUse<>)), As(typeof(IPublicUse1<>))]
        public static IServiceCollection AddService(this Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            return services;
        }
    }
}
