using System;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using JJMasterData.Commons.Options;

namespace JJMasterData.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static JJServiceBuilder AddJJMasterDataCore(this IServiceCollection services, string filePath = "appsettings.json")
    {
        var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile(filePath, optional: false, reloadOnChange: true)
        .Build();

        services.AddDefaultServices();
        return services.AddJJMasterDataCommons(configuration);
    }

    public static JJServiceBuilder AddJJMasterDataCore(this IServiceCollection services,
        Action<JJMasterDataOptions> configure)
    {
        services.AddDefaultServices();
        return services.AddJJMasterDataCommons(configure);
    }

    public static JJServiceBuilder AddJJMasterDataCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDefaultServices();
        return services.AddJJMasterDataCommons(configuration);
    }

    private static void AddDefaultServices(this IServiceCollection services)
    {
        services.AddScoped<IDataDictionaryRepository, DatabaseDataDictionaryRepository>();
        services.AddTransient<IExcelWriter, ExcelWriter>();
        services.AddTransient<ITextWriter, DataManager.Exports.TextWriter>();
    }
}
