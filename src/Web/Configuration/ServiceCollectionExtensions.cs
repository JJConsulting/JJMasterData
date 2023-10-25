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

public static class ServiceCollectionExtensions
{
    public static MasterDataServiceBuilder AddJJMasterDataWeb(this IServiceCollection services)
    {
        services.AddOptions<MasterDataWebOptions>();
        AddDefaultServices(services);
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
        AddDefaultServices(services);

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
        services.Configure(configureOptions);

        AddDefaultServices(services);

        return services.AddJJMasterDataCore(ConfigureJJMasterDataCoreOptions);

        void ConfigureJJMasterDataCoreOptions(MasterDataCoreOptions options)
        {
            options.ExportationFolderPath = webOptions.ExportationFolderPath;
            options.AuditLogTableName = webOptions.AuditLogTableName;
            options.DataDictionaryTableName = webOptions.DataDictionaryTableName;
            options.JJMasterDataUrl = webOptions.JJMasterDataUrl;
        }
    }

    private static void AddDefaultServices(IServiceCollection services)
    {
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
        services.AddRequestUrlCultureProvider();
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

    private static void AddRequestUrlCultureProvider(this IServiceCollection services,
        params CultureInfo[]? supportedCultures)
    {
        if (supportedCultures == null || supportedCultures.Length == 0)
        {
            supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("pt-BR"),
                new CultureInfo("zh-CN")
            };
        }

        services.Configure<RequestLocalizationOptions>(options =>
        {
            const string defaultCulture = "en-US";
            options.DefaultRequestCulture = new RequestCulture(defaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.AddInitialRequestCultureProvider(new CustomRequestCultureProvider(async context =>
            {
                string currentCulture = CultureInfo.CurrentCulture.Name;

                var segments = context.Request.Path.Value?.Split(new[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries);

                var culturePattern = new Regex(@"^[a-z]{2}(-[a-z]{2,4})?$",
                    RegexOptions.IgnoreCase);

                if (segments?.Length > 0 && culturePattern.IsMatch(segments[0]))
                {
                    currentCulture = segments[0];
                }

                var requestCulture = new ProviderCultureResult(currentCulture);

                return await Task.FromResult(requestCulture);
            }));
        });
    }
}