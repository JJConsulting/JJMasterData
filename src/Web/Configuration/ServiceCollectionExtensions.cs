using System.Diagnostics.CodeAnalysis;
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
using JJMasterData.Web.Services;
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

        AddMasterDataWebServices(services);

        return services.AddJJMasterDataCore(new MasterDataCoreOptionsConfiguration
        {
            ConfigureCommons = webOptionsConfiguration.ConfigureCommons,
            ConfigureCore = webOptionsConfiguration.ConfigureCore
        });
    }

    private static void AddMasterDataWebServices(IServiceCollection services)
    {
        services.AddOptions<MasterDataWebOptions>().BindConfiguration("JJMasterData");
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddTransient<IValidationDictionary, ModelStateWrapper>();
        services.AddTransient<RazorPartialRendererService>();
        services.AddTransient<LocalizationService>();
        services.AddScoped<SettingsService>();

        services.ConfigureWritableOptions<MasterDataCommonsOptions>("JJMasterData");
        services.ConfigureWritableOptions<MasterDataCoreOptions>("JJMasterData");
        services.ConfigureWritableOptions<MasterDataWebOptions>("JJMasterData");
        
        services.AddJJMasterDataWebOptimizer();

        services.AddControllersWithViews(options =>
            {
                options.ModelBinderProviders.Insert(0, new ExpressionModelBinderProvider());
            })
            .AddViewLocalization()
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (_, factory) =>
                    factory.Create(typeof(MasterDataResources));
            });

        services.AddHttpContextAccessor();
        services.AddSession();
        services.AddDistributedMemoryCache();
        services.AddActionFilters();
    }

    internal static void AddJJMasterDataWebOptimizer(this IServiceCollection services,
        Action<IAssetPipeline>? configure = null)
    {
        services.AddWebOptimizer(options =>
        {
            options.AddBundles();
            configure?.Invoke(options);
        });
    }
}