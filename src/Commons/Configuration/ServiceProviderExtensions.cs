using System;
using JJMasterData.Commons.DI;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Commons.Configuration;

public static class ServiceExtensions
{
    public static IServiceProvider UseJJMasterData(this IServiceProvider provider)
    {
        JJService.Provider = provider;
        return JJService.Provider;
    }
    
    public static T GetScopedDependentService<T>(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }
}