using JJMasterData.Commons.DI;
using JJMasterData.Commons.Options;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.DataDictionary.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Web.Authorization;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Models;
using JJMasterData.Web.Models.Abstractions;
using JJMasterData.Web.Services;
using JJMasterData.Web.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.RegularExpressions;
using JJMasterData.Core.Options;
using JJMasterData.Web.Options;

namespace JJMasterData.Web.Extensions;

public static class ServiceCollectionExtensions
{
    
    /// <summary>
    /// Adds all services you need to run a JJMasterData
    /// </summary>
    /// <param name="services"></param>
    /// <param name="filePath">
    /// Path relative to the base path stored in IConfigurationBuilder.Properties.
    /// </param>
    public static JJServiceBuilder AddJJMasterDataWeb(
        this IServiceCollection services,
        string filePath = "appsettings.json")
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(filePath, optional: false, reloadOnChange: true)
            .Build();

        AddDefaultServices(services);
        
        services.ConfigureWritableOptions<JJMasterDataCommonsOptions>(
            configuration.GetSection("JJMasterData"), filePath);
        services.ConfigureWritableOptions<JJMasterDataCoreOptions>(
            configuration.GetSection("JJMasterData"), filePath);
        services.ConfigureWritableOptions<JJMasterDataWebOptions>(
            configuration.GetSection("JJMasterData"), filePath);
        services.ConfigureWritableOptions<ConnectionStrings>(
            configuration.GetSection("ConnectionStrings"), filePath);
        services.ConfigureWritableOptions<ConnectionProviders>(
            configuration.GetSection("ConnectionProviders"), filePath);

        
        return services.AddJJMasterDataCore(configuration);
    }

    public static JJServiceBuilder AddJJMasterDataWeb(this IServiceCollection services, IConfigurationSection configuration)
    {
        AddDefaultServices(services);

        services.Configure<ConnectionString>(configuration.GetSection("ConnectionString"));
        services.Configure<ConnectionProviders>(configuration.GetSection("ConnectionProviders"));
        
        return services.AddJJMasterDataCore(configuration);
    }

    public static JJServiceBuilder AddJJMasterDataWeb(this IServiceCollection services,
        Action<JJConfigurationOptions> configureOptions)
    {
        var wrapper = new JJConfigurationOptions();

        configureOptions(wrapper);

        void ConfigureJJMasterDataCommonsOptions(JJMasterDataCommonsOptions options)
        {
            var wrapperOptions = wrapper.JJMasterDataCommons;
            options.SecretKey = wrapperOptions.SecretKey;
            options.PrefixGetProc = wrapperOptions.PrefixGetProc;
            options.PrefixSetProc = wrapperOptions.PrefixSetProc;
            options.ResourcesTableName = wrapperOptions.ResourcesTableName;
        }
        
        void ConfigureJJMasterDataCoreOptions(JJMasterDataCoreOptions options)
        {
            var wrapperOptions = wrapper.JJMasterDataCore;
            options.ExportationFolderPath = wrapperOptions.ExportationFolderPath;
            options.BootstrapVersion = wrapperOptions.BootstrapVersion;
            options.AuditLogTableName = wrapperOptions.AuditLogTableName;
            options.DataDictionaryTableName = wrapperOptions.DataDictionaryTableName;
            options.JJMasterDataUrl = wrapperOptions.JJMasterDataUrl;
        }
        
        void ConfigureJJMasterDataWebOptions(JJMasterDataWebOptions options)
        {
            var wrapperOptions = wrapper.JJMasterDataWeb;
            options.LayoutPath = wrapperOptions.LayoutPath;
            options.PopUpLayoutPath = wrapperOptions.PopUpLayoutPath;
        }

        void ConfigureConnectionStrings(ConnectionStrings options)
        {
            options.ConnectionString = wrapper.ConnectionStrings.ConnectionString;
        }

        void ConfigureConnectionProviders(ConnectionProviders options)
        {
            options.ConnectionString = wrapper.ConnectionProviders.ConnectionString;
        }

        services.Configure((Action<JJMasterDataCommonsOptions>)ConfigureJJMasterDataCommonsOptions);
        services.Configure((Action<JJMasterDataCoreOptions>)ConfigureJJMasterDataCoreOptions);
        services.Configure((Action<JJMasterDataWebOptions>)ConfigureJJMasterDataWebOptions);
        services.Configure((Action<ConnectionStrings>)ConfigureConnectionStrings);
        services.Configure((Action<ConnectionProviders>)ConfigureConnectionProviders);

        AddDefaultServices(services);

        return services.AddJJMasterDataCore(ConfigureJJMasterDataCoreOptions, ConfigureJJMasterDataCommonsOptions);
    }

    private static void AddDefaultServices(IServiceCollection services)
    {
        services.ConfigureOptions(typeof(PostConfigureStaticFileOptions));
        services.AddHttpContextAccessor();
        services.AddSession();
        services.AddSystemWebAdapters();
        services.AddDistributedMemoryCache();
        services.AddJJMasterDataServices();
        services.AddUrlRequestCultureProvider();
        services.AddAnonymousAuthorization();
    }

    internal static void AddAnonymousAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("MasterData",
                policy => policy.AddRequirements(new AllowAnonymousAuthorizationRequirement()));
            options.AddPolicy("DataDictionary",
                policy => policy.AddRequirements(new AllowAnonymousAuthorizationRequirement()));
            options.AddPolicy("Log", policy => policy.AddRequirements(new AllowAnonymousAuthorizationRequirement()));
        });
    }

    internal static void AddUrlRequestCultureProvider(this IServiceCollection services,
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

    internal static void AddJJMasterDataServices(this IServiceCollection services)
    {
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddTransient<IValidationDictionary, ModelStateWrapper>();

        services.AddTransient<ActionsService>();
        services.AddTransient<ApiService>();
        services.AddTransient<ElementService>();
        services.AddTransient<EntityService>();
        services.AddTransient<FieldService>();
        services.AddTransient<IndexesService>();
        services.AddTransient<UIOptionsService>();
        services.AddTransient<PanelService>();
        services.AddTransient<RelationsService>();
        services.AddTransient<ResourcesService>();
        services.AddTransient<RazorPartialRendererService>();
        services.AddTransient<ThemeService>();
        services.AddTransient<OptionsService>();
        services.AddTransient<AboutService>();
    }

    public static void ConfigureWritableOptions<T>(
        this IServiceCollection services,
        IConfigurationSection section,
        string file) where T : class, new()
    {
        services.Configure<T>(section);
        services.AddTransient<IWritableOptions<T>>(provider =>
        {
            var options = provider.GetService<IOptionsMonitor<T>>()!;
            return new WritableJsonOptions<T>(options, section.Key, file);
        });
    }
}