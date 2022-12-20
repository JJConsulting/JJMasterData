using System;
using JJMasterData.Commons.DI;

namespace JJMasterData.Commons.Extensions;

public static class ServiceExtensions
{
    public static IServiceProvider UseJJMasterData(this IServiceProvider provider)
    {
        JJService.Provider = provider;
        return JJService.Provider;
    }
}