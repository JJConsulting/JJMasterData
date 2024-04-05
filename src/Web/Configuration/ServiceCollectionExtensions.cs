using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Configuration;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Areas.DataDictionary.Services;
using JJMasterData.Web.Binders;
using JJMasterData.Web.Configuration.Options;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebOptimizer;

namespace JJMasterData.Web.Configuration;

public static class ServiceCollectionExtensions
{
    public static MasterDataServiceBuilder AddJJMasterDataWeb(this IServiceCollection services)
    {
        AddMasterDataWebServices(services);
        return services.AddJJMasterDataCore();
    }

    /// <summary>
    /// Adds all services you need to run a JJMasterData
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static MasterDataServiceBuilder AddJJMasterDataWeb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddMasterDataWebServices(services);

        return services.AddJJMasterDataCore(configuration);
    }
    
    public static MasterDataServiceBuilder AddJJMasterDataWeb(
        this IServiceCollection services,
        MasterDataWebOptionsConfiguration webOptionsConfiguration
    )
    {
        if (webOptionsConfiguration.ConfigureWeb != null) 
            services.PostConfigure(webOptionsConfiguration.ConfigureWeb);

        AddMasterDataWebServices(services, webOptionsConfiguration);

        return services.AddJJMasterDataCore(new MasterDataCoreOptionsConfiguration
        {
            ConfigureCommons = webOptionsConfiguration.ConfigureCommons,
            ConfigureCore = webOptionsConfiguration.ConfigureCore
        });
    }
    
    public static MasterDataServiceBuilder AddJJMasterDataWeb(
        this IServiceCollection services,
        Action<MasterDataWebOptionsConfiguration> webOptionsConfiguration
    )
    {
        var configuration = new MasterDataWebOptionsConfiguration();

        webOptionsConfiguration(configuration);
        
        if (configuration.ConfigureWeb != null) 
            services.PostConfigure(configuration.ConfigureWeb);

        AddMasterDataWebServices(services, configuration);

        return services.AddJJMasterDataCore(new MasterDataCoreOptionsConfiguration
        {
            ConfigureCommons = configuration.ConfigureCommons,
            ConfigureCore = configuration.ConfigureCore
        });
    }


    private static void AddMasterDataWebServices(
        IServiceCollection services, 
        MasterDataWebOptionsConfiguration? configuration = null)
    {
        services.AddMvcServices(configuration ?? new());
        
        services.AddOptions<MasterDataWebOptions>().BindConfiguration("JJMasterData");
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddTransient<IValidationDictionary, ModelStateWrapper>();
        services.AddTransient<LocalizationService>();
        services.AddScoped<SettingsService>();

        services.ConfigureWritableOptions<MasterDataCommonsOptions>("JJMasterData");
        services.ConfigureWritableOptions<MasterDataCoreOptions>("JJMasterData");
        services.ConfigureWritableOptions<MasterDataWebOptions>("JJMasterData");
        
        services.AddMasterDataWebOptimizer();

        services.AddHttpContextAccessor();
        services.AddSession();
        services.AddDistributedMemoryCache();
        services.AddActionFilters();
    }

    private static void AddMvcServices(
        this IServiceCollection services,
        MasterDataWebOptionsConfiguration configuration)
    {
        services.AddControllersWithViews(options =>
            {
                options.ModelBinderProviders.Add(new ExpressionModelBinderProvider());
            })
            .AddViewLocalization()
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                {
                    var assemblyName = type.Assembly.GetName().Name;
                    if(assemblyName is "JJMasterData.Commons" or "JJMasterData.Core" or "JJMasterData.Web")
                        return factory.Create(typeof(MasterDataResources));
                    
                    return configuration.ConfigureDataAnnotations(type,factory);
                };
            });
    }

    internal static void AddMasterDataWebOptimizer(this IServiceCollection services,
        Action<IAssetPipeline>? configure = null)
    {
        services.AddWebOptimizer(options =>
        {
            options.AddBundles();
            configure?.Invoke(options);
        });
    }
}