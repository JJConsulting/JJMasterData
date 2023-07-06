using System;
using JJMasterData.Commons.DI;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Commons.Configuration;

public class LazyService<T> : Lazy<T> where T : class
{
    public LazyService(IServiceScopeFactory scopeFactory)
        : base(() =>
        {
            var scope = scopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<T>();
        })
    {
    }
}