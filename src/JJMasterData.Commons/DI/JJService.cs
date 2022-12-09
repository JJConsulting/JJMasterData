using System;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Options;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.DI;

public static class JJService
{
    public static IServiceProvider Provider { get; internal set; }

    public static IEntityRepository EntityRepository
    {
        get
        {
            using var scope = Provider.CreateScope();
            return scope.ServiceProvider.GetService<IEntityRepository>();
        }
    }
    
    public static JJMasterDataOptions Options
    {
        get
        {
            using var scope = Provider.CreateScope();
            return scope.ServiceProvider.GetService<IOptionsSnapshot<JJMasterDataOptions>>()!.Value;
        }
    }

    public static IBackgroundTask BackgroundTask => Provider.GetService<IBackgroundTask>();
    public static ILocalizationProvider LocalizationProvider
    {
        get 
        { 
            using var scope = Provider.CreateScope();
            return scope.ServiceProvider.GetService<ILocalizationProvider>()!;
        }
    }

    public static ILogger Logger => Provider.GetRequiredService<ILogger<Logger>>();
}
