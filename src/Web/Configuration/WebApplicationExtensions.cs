using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Web.Configuration.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Web.Configuration;

public static partial class WebApplicationExtensions
{
    [PublicAPI]
    [Obsolete("Please handle request localization at your application.")]
    public static WebApplication UseUrlRequestLocalization(this WebApplication app, Action<MasterDataLocalizationOptions>? configureLocalization = null)
    {
        app.UseRequestLocalization(options =>
        {
            var routingOptions = new MasterDataLocalizationOptions();
            configureLocalization?.Invoke(routingOptions);
            
            var supportedCultures = new List<CultureInfo>
            {
                new("en-US"),
                new("pt-BR"),
                new("zh-CN")
            };
            
            supportedCultures.AddRange(routingOptions.AdditionalCultures);
        
            options.DefaultRequestCulture = new RequestCulture(routingOptions.DefaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        
            options.AddInitialRequestCultureProvider(new CustomRequestCultureProvider(context =>
            {
                string currentCulture;
            
                var segments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
                if (segments?.Length > 0 && CultureRegex().IsMatch(segments[0]))
                {
                    currentCulture = segments[0];
                }
                else
                {
                    currentCulture = routingOptions.DefaultCulture;
                }
            
                var requestCulture = new ProviderCultureResult(currentCulture);
            
                return Task.FromResult(requestCulture)!;
            }));
        });

        return app;
    }

   
    [Obsolete("Please use both MapDataDictionary and MapMasterData instead.")]
    public static ControllerActionEndpointConventionBuilder MapJJMasterData(this WebApplication app,
        Action<MasterDataAreaOptions>? configure = null)
    {
        var options = new MasterDataAreaOptions();

        configure?.Invoke(options);

        var pattern = "{area}/{controller}/{action}/{elementName?}/{fieldName?}/{id?}";

        if (options.Prefix is not null)
            pattern = $"{options.Prefix.Trim('/')}/{pattern}";

        if (options.EnableCultureProvider)
            pattern = $"/{{culture}}/{pattern}";
        
        return app.MapControllerRoute(
            name: "JJMasterData",
            pattern: pattern);
    }

    [PublicAPI]
    public static async Task CreateStructureIfNotExistsAsync(this WebApplication app)
    {
        using var dbScope = app.Services.CreateScope();
        var serviceProvider = dbScope.ServiceProvider;
    
        var dataDictionaryRepository = serviceProvider.GetRequiredService<IDataDictionaryRepository>();
        await dataDictionaryRepository.CreateStructureIfNotExistsAsync();
    }
    
    public static ControllerActionEndpointConventionBuilder MapDataDictionary(this WebApplication app)
    {
        return app.MapAreaControllerRoute(
            name: "dataDictionary",
            areaName: "DataDictionary",
            pattern: "DataDictionary/{controller=Element}/{action=Index}/{elementName?}/{fieldName?}");
    }
    
    public static ControllerActionEndpointConventionBuilder MapMasterData(this WebApplication app)
    {
        return app.MapAreaControllerRoute(
            name: "masterData",
            areaName: "MasterData",
            pattern:  "MasterData/{controller}/{action}/{elementName?}/{fieldName?}/{id?}");
    }

    [GeneratedRegex("^[a-z]{2}(-[a-z]{2,4})?$", RegexOptions.IgnoreCase, "pt-BR")]
    private static partial Regex CultureRegex();
}