using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.DataDictionary.Services.Abstractions;
using JJMasterData.Web.Authorization;
using JJMasterData.Web.Filters;
using JJMasterData.Web.Models;
using JJMasterData.Web.Models.Abstractions;
using JJMasterData.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.RegularExpressions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Options;
using JJMasterData.Core.Extensions;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Hosting;

namespace JJMasterData.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static JJServiceBuilder AddJJMasterDataWeb(this IServiceCollection services)
    {
        services.AddOptions<JJMasterDataOptions>();
        services.ConfigureOptions(typeof(PostConfigureStaticFileOptions));
        services.AddHttpContextAccessor();
        services.AddSession();
        services.AddSystemWebAdaptersServices();
        services.AddDistributedMemoryCache();
        services.AddJJMasterDataServices();
        services.AddUrlRequestCultureProvider();
        services.AddAnonymousAuthorization();

        return services.AddJJMasterDataCore();
    }

    public static JJServiceBuilder AddJJMasterDataWeb(this IServiceCollection services, IConfiguration configuration,
        string settingsPath = "appsettings.json")
    {
        services.ConfigureWritableOptions<JJMasterDataOptions>(
            configuration.GetSection("JJMasterData"), settingsPath);
        services.ConfigureWritableOptions<ConnectionStrings>(
            configuration.GetSection("ConnectionStrings"), settingsPath);
        services.ConfigureWritableOptions<ConnectionProviders>(
            configuration.GetSection("ConnectionProviders"), settingsPath);

        return AddJJMasterDataWeb(services);
    }

    public static JJServiceBuilder AddJJMasterDataWeb(this IServiceCollection services,
        Action<JJOptions> configureOptions)
    {
        var jjOptions = new JJOptions();
        
        configureOptions(jjOptions);


        void MasterDataOptions(JJMasterDataOptions options)
        {
            jjOptions.JJMasterDataOptions = options;
        }

        void ConnectionStrings(ConnectionStrings connectionStrings)
        {
            jjOptions.ConnectionStrings = connectionStrings;
        }

        void ConnectionProviders(ConnectionProviders connectionProviders)
        {
            jjOptions.ConnectionProviders = connectionProviders;
        }

        services.Configure((Action<JJMasterDataOptions>)MasterDataOptions);
        services.Configure((Action<ConnectionStrings>)ConnectionStrings);
        services.Configure((Action<ConnectionProviders>)ConnectionProviders);

        return AddJJMasterDataWeb(services);
    }

    internal static void AddSystemWebAdaptersServices(this IServiceCollection services)
    {
        services.AddSystemWebAdapters();
        services.AddTransient<ResponseEndFilter>();
        services.AddOptions<MvcOptions>()
            .Configure(options =>
            {
                // We want the check for HttpResponse.End() to be done as soon as possible after the action is run.
                // This will minimize any chance that output will be written which will fail since the response has completed.
                options.Filters.Add<ResponseEndFilter>(int.MaxValue);
            });
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
            string defaultCulture = "en-US";
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

                if (segments?.Length > 0 && culturePattern.IsMatch(segments![0]))
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
            return new WritableOptions<T>(options, section.Key, file);
        });
    }
}