using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.Html;
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

    public static MasterDataServiceBuilder AddJJMasterDataCore(
        this IServiceCollection services,
        MasterDataCoreOptionsConfiguration optionsConfiguration
        )
    {
        if (optionsConfiguration.ConfigureCore != null) 
            services.PostConfigure(optionsConfiguration.ConfigureCore);

        services.AddMasterDataCoreServices();
        return services.AddJJMasterDataCommons(optionsConfiguration.ConfigureCommons);
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

        services.AddTransient(typeof(HtmlTemplateRenderer<>));
        
        services.AddScoped<IExcelWriter, ExcelWriter>();
        services.AddScoped<ITextWriter, TextWriter>();

        services.AddFactories();
    }
}