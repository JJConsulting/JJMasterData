using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;


namespace JJMasterData.Commons.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AllowLazyInicialization(this IServiceCollection services) 
    {
        var lastRegistration = services.Last();
        
        var lazyServiceType = typeof(Lazy<>).MakeGenericType(
            lastRegistration.ServiceType);
        
        var lazyServiceImplementationType = typeof(LazyService<>).MakeGenericType(
            lastRegistration.ServiceType);
        
        services.Add(new ServiceDescriptor(lazyServiceType, lazyServiceImplementationType,lastRegistration.Lifetime));
        return services;
    }
}