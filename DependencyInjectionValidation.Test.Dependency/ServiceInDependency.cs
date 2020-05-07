using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjectionValidation.Test.Dependency
{
#pragma warning disable S2326 // Unused type parameters should be removed
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable S3453 // Classes should not have only "private" constructors
    public static class ServiceExtensions
    {
        [Exposes(typeof(OtherAssemblyDependingOnServiceInPublicConstructor)), As(typeof(IOtherAssemblyPublicUse1))]
        public static IServiceCollection AddOtherAssemblyDependingOnServiceInPublicConstructor(this IServiceCollection services) => services;
        
        [Exposes(typeof(OtherAssemblyDependingOnServiceInInternalConstructor)), As(typeof(IOtherAssemblyPublicUse1))]
        public static IServiceCollection AddOtherAssemblyDependingOnServiceInInternalConstructor(this IServiceCollection services) => services;
        
        [Exposes(typeof(OtherAssemblyDependingOnServiceInPublicConstructor<string, object>)), As(typeof(IOtherAssemblyPublicUse1<string>))]
        public static IServiceCollection AddOtherAssemblyDependingOnServiceInPublicConstructorFixedParameters(this IServiceCollection services) => services;
        
        [Exposes(typeof(OtherAssemblyDependingOnServiceInPublicConstructor<,>)), As(typeof(IOtherAssemblyPublicUse1<>))]
        public static IServiceCollection AddOtherAssemblyDependingOnServiceInPublicConstructorBothFreeParameters<U1, U2>(this IServiceCollection services) => services;

        [Exposes(typeof(OtherAssemblyDependingOnServiceInPublicConstructor<,>)), As(typeof(IOtherAssemblyPublicUse1<>))]
        public static IServiceCollection AddOtherAssemblyDependingOnServiceInPublicConstructorFirstFreeParameters<U2>(this IServiceCollection services) => services;
        
        [Exposes(typeof(OtherAssemblyDependingOnServiceInPublicConstructor<,>)), As(typeof(IOtherAssemblyPublicUse1<object>))]
        public static IServiceCollection AddOtherAssemblyDependingOnServiceInPublicConstructorSecondFreeParameters<U1>(this IServiceCollection services) => services;

        [Exposes(typeof(OtherAssemblyImplicitUse)), As(typeof(IOtherAssemblyPublicUse1))]
        public static IServiceCollection AddOtherAssemblyImplicitUse(this IServiceCollection services) => services;
        
        [Exposes(typeof(OtherAssemblyImplicitUse<System.Type>)), As(typeof(IOtherAssemblyPublicUse1<System.Type>))]
        public static IServiceCollection AddOtherAssemblyImplicitUseOneFixedParameter(this IServiceCollection services) => services;
        
        [Exposes(typeof(OtherAssemblyImplicitUse<>)), As(typeof(IOtherAssemblyPublicUse1<>))]
        public static IServiceCollection AddOtherAssemblyImplicitUseOneFreeParameter<V>(this IServiceCollection services) => services;
        
        [Exposes(typeof(OtherAssemblyImplicitUse<string, object>)), As(typeof(IOtherAssemblyPublicUse1<string, object>))]
        public static IServiceCollection AddOtherAssemblyImplicitUseTwoFixedParameters(this IServiceCollection services) => services;
        
        [Exposes(typeof(OtherAssemblyImplicitUse<,>)), As(typeof(IOtherAssemblyPublicUse1<,>))]
        public static IServiceCollection AddOtherAssemblyImplicitUseTwoFreeParameters<V1, V2>(this IServiceCollection services, string parameter) => services;
    }
    #region public interfaces
    public interface IOtherAssemblyPublicUse1
    {
    }
    public interface IOtherAssemblyPublicUse1<T>
    {
    }
    public interface IOtherAssemblyPublicUse1<T1, T2>
    {
    }
    public interface IOtherAssemblyPublicUse2
    {
    }
    public interface IOtherAssemblyPublicUse2<T>
    {
    }
    public interface IOtherAssemblyPublicUse2<T1, T2>
    {
    }
    #endregion
    #region internal interfaces
    internal interface IOtherAssemblyInternalUse
    {
    }
    internal interface IOtherAssemblyInternalUse<T>
    {
    }
    internal interface IOtherAssemblyInternalUse<T1, T2>
    {
    }
    #endregion
    #region class is public, base interface is public, no constructor dependency
    public class OtherAssemblyPublicUse : IOtherAssemblyPublicUse1 // can be constructed freely
    {
    }
    public class OtherAssemblyPublicUse<U> : IOtherAssemblyPublicUse1<U> // can be constructed freely
    {
    }
    public class OtherAssemblyPublicUse<U1, U2> : IOtherAssemblyPublicUse1<U1, U2> // can be constructed freely
    {
    }
    #endregion
    #region class is public, base interface is public, non-private constructor dependency => service extension
    public class OtherAssemblyDependingOnServiceInPublicConstructor : IOtherAssemblyPublicUse1 // cannot be constructed freely, needs service extension
    {
        public OtherAssemblyDependingOnServiceInPublicConstructor(IOtherAssemblyPublicUse2 internal_service)
        {

        }
    }
    public class OtherAssemblyDependingOnServiceInInternalConstructor : IOtherAssemblyPublicUse1 // cannot be constructed freely, needs service extension
    {
        internal OtherAssemblyDependingOnServiceInInternalConstructor(IOtherAssemblyPublicUse2 internal_service)
        {

        }
    }
    public class OtherAssemblyDependingOnServiceInPublicConstructor<U1, U2> : IOtherAssemblyPublicUse1<U1> // cannot be constructed freely, needs service extension
    {
        public OtherAssemblyDependingOnServiceInPublicConstructor(IOtherAssemblyPublicUse2<U2> internal_service)
        {

        }
    }
    #endregion
    #region class is public, base interface is public, private constructor dependency
    public class OtherAssemblyDependingOnServiceInPrivateConstructor : IOtherAssemblyPublicUse1 // cannot be constructed freely, but also cannot be exposed with service extension
    {
        private OtherAssemblyDependingOnServiceInPrivateConstructor(IOtherAssemblyPublicUse2 internal_service)
        {

        }
    }
    public class OtherAssemblyDependingOnServiceInPrivateConstructor<U1, U2> : IOtherAssemblyPublicUse1<U1> // cannot be constructed freely, but also cannot be exposed with service extension
    {
        private OtherAssemblyDependingOnServiceInPrivateConstructor(IOtherAssemblyPublicUse2<U1, U2> internal_service)
        {

        }
    }
    #endregion
    #region class is internal, base interface is internal
    internal class OtherAssemblyInternalUse : IOtherAssemblyInternalUse // is not used outside this assembly
    {
    }
    internal class OtherAssemblyInternalUse<U> : IOtherAssemblyInternalUse<U> // is not used outside this assembly
    {
    }
    internal class OtherAssemblyInternalUse<U1, U2> : IOtherAssemblyInternalUse<U1, U2> // is not used outside this assembly
    {
    }
    #endregion
    #region class is internal, base interface is public => service extension
    internal class OtherAssemblyImplicitUse : IOtherAssemblyPublicUse1 // must be exposed through service extension
    {
    }
    internal class OtherAssemblyImplicitUse<U> : IOtherAssemblyPublicUse1<U> // must be exposed through service extension
    {
    }
    internal class OtherAssemblyImplicitUse<U1, U2> : IOtherAssemblyPublicUse1<U1, U2> // must be exposed through service extension
    {
    }
    #endregion
    public class JustAClass
    {

    }
#pragma warning restore S3453 // Classes should not have only "private" constructors
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore S2326 // Unused type parameters should be removed
}
