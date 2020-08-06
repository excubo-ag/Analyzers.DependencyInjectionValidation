
# Excubo.Analyzers.DependencyInjectionValidation

This roslyn analyzer validates the use of dependency injection (DI) at compile time. It aims to reduce the number of times applications are run when not all dependencies are registered with the service provider.
The analysis is performed using class and method attributes. Using it requires the package [Excubo.Analyzers.Annotations](https://github.com/excubo-ag/Analyzers.Annotations) to be installed as well.

# Installation

Excubo.Analyzers.DependencyInjectionValidation is distributed via [via nuget.org](https://www.nuget.org/packages/Excubo.Analyzers.DependencyInjectionValidation/).
![Nuget](https://img.shields.io/nuget/v/Excubo.Analyzers.DependencyInjectionValidation)

#### Package Manager:
```ps
Install-Package Excubo.Analyzers.DependencyInjectionValidation -Version 1.0.6
Install-Package Excubo.Analyzers.Annotations -Version 1.0.5
```

#### .NET Cli:
```cmd
dotnet add package Excubo.Analyzers.DependencyInjectionValidation --version 1.0.6
dotnet add package Excubo.Analyzers.Annotations --version 1.0.5
```

# How to use

Imagine your app is using a service called `IService`. An implementation of this service needs to be added to the service collection, otherwise the app can't run. This analyzer helps to detect such situations before the application is run.

Here's the service implementing `IService`:

```cs
namespace Foo
{
   public class Service : IService
   {
   }
}
```

As long as you haven't added any annotations, you will get the following error message

```cs
namespace Foo
{
   public class Service : IService
                ~~~~~~~
                ^ Missing service extension for class Service.
   {
   }
}
```

If this is a class library, this is exactly what you should add: your users don't want to know how your service needs to be added, they just want to add it.

The solution is to write a service extension method, e.g.

```cs
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Foo
{
    public static class ServiceExtension
    {
        [Exposes(typeof(Service)), As(typeof(IService))]
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            // add any dependencies of Service here as well.
            return services.AddSingleton<IService, Service>();
        }
    }
}
```

Note the attributes on the extension method. They are used by this analyzer to validate whether everything seems fine. With `Expose(typeof(Service))` you tell the analyzer to match this extension method up with the class `Service`. `As(typeof(IService))` is used by the analyzer to see which services are implemented here.

If this is not a class library project, you should add the `DependencyInjectionPoint` attribute to the method that adds all your services (usually in a file called `Startup.cs`)

```cs
namespace Application
{
    public class Startup
    {
        [DependencyInjectionPoint]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IService, Service>();
        }
    }
}
```

Everything should be fine so far. But what if you later add a service dependency to your class?

```cs

namespace Foo
{
   public class Service : IService
   {
       public Service(ISomeDependency dependency)
       {
          //....
       }
   }
}
```

Normally, you would run into the issue that this dependency is not yet registered. Excubo.Analyzers.DependencyInjectionValidation now warns you that this is not done yet:

```cs
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Foo
{
    public static class ServiceExtension
    {
        [Exposes(typeof(Service)), As(typeof(IService))]
        public static IServiceCollection AddService(this IServiceCollection services)
                                         ~~~~~~~~~~
                                         ^ Service extension is not adding all required interfaces for Service. Missing interface: ISomeDependency.
        {
            // add any dependencies of Service here as well.
            return services.AddSingleton<IService, Service>();
        }
    }
}
```

or if you're in the application

```cs
namespace Application
{
    public class Startup
    {
        [DependencyInjectionPoint]
        public void ConfigureServices(IServiceCollection services)
                    ~~~~~~~~~~~~~~~~~
                    ^ Dependency ISomeDependency of Service is missing.
        {
            services.AddSingleton<IService, Service>();
        }
    }
}
```

This is now straightforward to fix!

```cs
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Foo
{
    public static class ServiceExtension
    {
        [Exposes(typeof(Service)), As(typeof(IService))]
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            // add any dependencies of Service here as well.
            return services
                .AddSingleton<ISomDependency, SomeDependency>()
                .AddSingleton<IService, Service>();
        }
    }
}
```

or if you're in the application

```cs
namespace Application
{
    public class Startup
    {
        [DependencyInjectionPoint]
        public void ConfigureServices(IServiceCollection services)
        {
            services
              .AddSingleton<ISomDependency, SomeDependency>()
              .AddSingleton<IService, Service>();
        }
    }
}
```


# Notes

- This is the first public release of the analyzer. If you experience any issue, please raise an issue in this repository.

- The analyzer runs on compilation. That means your diagnostics won't refresh until you rebuild the project.

- Obviously this can only work with types that the analyzer knows about. If you consume services that are defined in another assembly (e.g. `ILogger`), and the authors of that library didn't add the attributes on their service extension methods, this analyzer won't be able to find them.
This can be worked aroung with the `IgnoreDependency` and the `Injects` attributes.

Examples: 

## ignore a dependency, because it should stay in the responsibility of the user

```cs
using Excubo.Analyzers.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Foo
{
    public static class ServiceExtension
    {
        [Exposes(typeof(Service)), As(typeof(IService))]
        [IgnoreDependency(typeof(ILogger<Service>))] // we know we want this dependency, but we don't want to add it here
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            // add any dependencies of Service here as well.
            return services
                .AddSingleton<ISomDependency, SomeDependency>()
                .AddSingleton<IService, Service>();
        }
    }
}
```

## mark a dependency as handled, even if it isn't automatically detected by the analyzer (e.g. non-annotated service extension from third party)

```cs
namespace Application
{
    public class Startup
    {
        [DependencyInjectionPoint]
        [Injects(typeof(ILogger<>))]
        public void ConfigureServices(IServiceCollection services)
        {
            services
              .AddLogging()
              .AddSingleton<ISomDependency, SomeDependency>()
              .AddSingleton<IService, Service>();
        }
    }
}
```
