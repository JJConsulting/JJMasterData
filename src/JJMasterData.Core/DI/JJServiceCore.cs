using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DI;

/// <summary>
/// Static service acessor. Will be removed in the next version.
/// </summary>
public static class JJServiceCore 
{
   
    public static IDataDictionaryRepository DataDictionaryRepository
    {
        get
        {
            using var scope = JJService.Provider.CreateScope();
            return scope.ServiceProvider.GetService<IDataDictionaryRepository>();
        }
    }
    
    public static IFormEventResolver FormEventResolver
    {
        get
        {
            using var scope = JJService.Provider.CreateScope();
            return scope.ServiceProvider.GetService<IFormEventResolver>();
        }
    }
    
    public static JJMasterDataCoreOptions Options
    {
        get
        {
            using var scope = JJService.Provider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IOptions<JJMasterDataCoreOptions>>().Value;
        }
    }
    
}