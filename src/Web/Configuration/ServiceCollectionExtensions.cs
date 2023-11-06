using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebOptimizer;

namespace JJMasterData.Web.Configuration;

public static partial class ServiceCollectionExtensions
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

        services.ConfigureWritableOptions<MasterDataCommonsOptions>(
            configuration.GetJJMasterData());
        services.ConfigureWritableOptions<MasterDataCoreOptions>(
            configuration.GetJJMasterData());
        services.ConfigureWritableOptions<MasterDataWebOptions>(
            configuration.GetJJMasterData());

        return services.AddJJMasterDataCore(configuration);
    }
    
    public static MasterDataServiceBuilder AddJJMasterDataWeb(
        this IServiceCollection services,
        Action<MasterDataWebOptions> configureOptions
    )
    {
        var webOptions = new MasterDataWebOptions();

        configureOptions(webOptions);
        services.PostConfigure(configureOptions);
        
        AddMasterDataWebServices(services);

        return services.AddJJMasterDataCore(ConfigureJJMasterDataCoreOptions);

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
        void ConfigureJJMasterDataCoreOptions(MasterDataCoreOptions options)
        {
            if (webOptions.ExportationFolderPath != null)
                options.ExportationFolderPath = webOptions.ExportationFolderPath;
            if (webOptions.AuditLogTableName != null) 
                options.AuditLogTableName = webOptions.AuditLogTableName;

            if (webOptions.DataDictionaryTableName != null)
                options.DataDictionaryTableName = webOptions.DataDictionaryTableName;

            if (webOptions.MasterDataUrl != null) 
                options.MasterDataUrl = webOptions.MasterDataUrl;

            if (webOptions.ConnectionString != null) 
                options.ConnectionString = webOptions.ConnectionString;
            
            if(webOptions.ConnectionProvider != default)
                options.ConnectionProvider = webOptions.ConnectionProvider;
            
            if (webOptions.LocalizationTableName != null)
                options.LocalizationTableName = webOptions.LocalizationTableName;
            if (webOptions.ReadProcedurePattern != null) 
                options.ReadProcedurePattern = webOptions.ReadProcedurePattern;
            if (webOptions.WriteProcedurePattern != null)
                options.WriteProcedurePattern = webOptions.WriteProcedurePattern;
            if (webOptions.SecretKey != null) 
                options.SecretKey = webOptions.SecretKey;
        }
    }

    private static void AddMasterDataWebServices(IServiceCollection services)
    {
        services.AddOptions<MasterDataWebOptions>().BindConfiguration("JJMasterData");
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddTransient<IValidationDictionary, ModelStateWrapper>();
        services.AddTransient<RazorPartialRendererService>();
        services.AddTransient<OptionsService>();
        services.AddTransient<LocalizationService>();

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