using JJMasterData.Commons.DI;
using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static JJServiceBuilder AddJJMasterDataCore(this IServiceCollection services)
    {
        services.AddJJMasterDataServices();   
        return services.AddJJMasterDataCommons();
    }

    private static void AddJJMasterDataServices(this IServiceCollection services)
    {
        services.AddScoped<IDictionaryRepository, DictionaryDao>();
        services.AddTransient<IExcelWriter, ExcelWriter>();
        services.AddTransient<ITextWriter, TextWriter>();
    }
}
