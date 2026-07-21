using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Configuration;
using JJMasterData.Core.Abstractions;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Web.Components;
using JJMasterData.Web.Configuration.Options;
using JJMasterData.Web.DataDictionary.Services;
using JJMasterData.Web.DataManager.Services;
using JJMasterData.Web.Events;
using JJMasterData.Web.Events.Abstractions;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Models;
using JJMasterData.Web.Services;
using JJMasterData.Web.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


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
        services.Configure<CookieTempDataProviderOptions>(options => {
            options.Cookie.IsEssential = true;
        }); 
        
        services.AddMvcServices(configuration ?? new());

        services.AddLocalization();
        
        services.AddMemoryCache();
        
        services.AddOptions<MasterDataWebOptions>().BindConfiguration("JJMasterData");
        services.AddScoped<IValidationDictionary, ModelStateWrapper>();

        services.AddTransient<JJMasterDataLogo>();
        
        services.AddHttpContextAccessor();
        services.AddScoped<IMasterDataRequestContext, AspNetCoreMasterDataRequestContext>();
        services.AddScoped<ComponentFileLinkProvider>();
        services.AddScoped<IFileUrlProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<ComponentFileLinkProvider>());
        services.AddActionFilters();
        services.AddHttpServices();
        services.AddActionServices();
        services.AddFactories();
        services.AddTransient<ElementViewService>();
        services.AddTransient<LookupRequestService>();
        services.AddTransient<UrlRedirectService>();
        services.AddTransient<UploadAreaManager>();
        services.AddTransient<UploadViewManager>();
        services.AddTransient<AuditLogFormElementFactory>();
        services.AddScoped<IGridEventHandlerResolver, GridEventHandlerResolver>();
    }
    
    private static void AddMvcServices(
        this IServiceCollection services,
        MasterDataWebOptionsConfiguration configuration)
    {
        services.AddControllersWithViews()
            .AddViewLocalization()
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                {
                    var assemblyName = type.Assembly.FullName;
                    if(assemblyName?.Contains("JJMasterData") == true)
                        return factory.Create(typeof(MasterDataResources));
                    
                    return configuration.ConfigureDataAnnotations(type,factory);
                };
            });
    }
}
