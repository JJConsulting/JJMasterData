using System;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Options;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.DI;

public static class JJService
{
    public static IServiceProvider Provider { get; internal set; }

    public static JJMasterDataCommonsOptions CommonsOptions
    {
        get
        {
            using var scope = Provider.CreateScope();
            return scope.ServiceProvider.GetService<IOptionsSnapshot<JJMasterDataCommonsOptions>>()!.Value;
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
}
