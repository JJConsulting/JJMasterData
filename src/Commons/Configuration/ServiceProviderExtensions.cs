using System;
using JJMasterData.Commons.DI;

namespace JJMasterData.Commons.Configuration;

public static class ServiceExtensions
{
    public static IServiceProvider UseJJMasterData(this IServiceProvider provider)
    {
        JJService.Provider = provider;
        return JJService.Provider;
    }
}