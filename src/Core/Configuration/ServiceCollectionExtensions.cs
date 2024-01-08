using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class ServiceCollectionExtensions
{
    public static MasterDataServiceBuilder AddJJMasterDataCore(this IServiceCollection services)
    {
        services.AddMasterDataCoreServices();

        return services.AddJJMasterDataCommons();
    }

    public static MasterDataServiceBuilder AddJJMasterDataCore(this IServiceCollection services,
        Action<MasterDataCoreOptions> configureCore)
    {
        var coreOptions = new MasterDataCoreOptions();

        configureCore(coreOptions);

        services.PostConfigure(configureCore);

        services.AddMasterDataCoreServices();
        return services.AddJJMasterDataCommons(ConfigureJJMasterDataCommonsOptions);

        void ConfigureJJMasterDataCommonsOptions(MasterDataCommonsOptions options)
        {
            if (coreOptions.ConnectionString != null)
                options.ConnectionString = coreOptions.ConnectionString;

            if (coreOptions.ConnectionProvider != default)
                options.ConnectionProvider = coreOptions.ConnectionProvider;

            if (coreOptions.LocalizationTableName != null)
                options.LocalizationTableName = coreOptions.LocalizationTableName;

            if (coreOptions.ReadProcedurePattern != null)
                options.ReadProcedurePattern = coreOptions.ReadProcedurePattern;

            if (coreOptions.WriteProcedurePattern != null)
                options.WriteProcedurePattern = coreOptions.WriteProcedurePattern;

            if (coreOptions.SecretKey != null)
                options.SecretKey = coreOptions.SecretKey;
        }
    }

    public static MasterDataServiceBuilder AddJJMasterDataCore(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MasterDataCoreOptions>(configuration.GetJJMasterData());

        services.AddMasterDataCoreServices();

        return services.AddJJMasterDataCommons(configuration);
    }

    private static void AddMasterDataCoreServices(this IServiceCollection services)
    {
        services.AddOptions<MasterDataCoreOptions>().BindConfiguration("JJMasterData");

        services.AddHttpServices();
        services.AddDataDictionaryServices();
        services.AddDataManagerServices();
        services.AddEventHandlers();
        services.AddExpressionServices();
        services.AddActionServices();

        services.AddScoped<IDataDictionaryRepository, SqlDataDictionaryRepository>();

        services.AddScoped<IExcelWriter, ExcelWriter>();
        services.AddScoped<ITextWriter, TextWriter>();

        services.AddFactories();
    }
}