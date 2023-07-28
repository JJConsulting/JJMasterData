using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;


namespace JJMasterData.Commons.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Allow the service to be loaded only when needed. See https://stackoverflow.com//questions/44934511/does-net-core-dependency-injection-support-lazyt#answer-76630697.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AllowLazyInitialization(this IServiceCollection services) 
    {
        var lastRegistration = services.Last();
        
        var lazyServiceType = typeof(Lazy<>).MakeGenericType(
            lastRegistration.ServiceType);
        
        var lazyServiceImplementationType = typeof(LazyService<>).MakeGenericType(
            lastRegistration.ImplementationType);
        
        services.Add(new ServiceDescriptor(lazyServiceType, lazyServiceImplementationType,lastRegistration.Lifetime));
        return services;
    }
}