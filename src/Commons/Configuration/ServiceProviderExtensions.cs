#if NET48
using System;
using JJMasterData.Commons.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Commons.Configuration;

public static class ServiceProviderExtensions
{
    public static IServiceProvider UseJJMasterData(this IServiceProvider provider)
    {
        StaticServiceLocator.Provider = provider;
        return StaticServiceLocator.Provider;
    }
    
    public static T GetScopedDependentService<T>(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }
}
#endif  