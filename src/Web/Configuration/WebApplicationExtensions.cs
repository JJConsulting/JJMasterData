using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using JJMasterData.Web.Configuration.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.FileProviders;

namespace JJMasterData.Web.Configuration;

public static class WebApplicationExtensions
{
    public static WebApplication UseJJMasterDataWeb(this WebApplication app, Action<RequestLocalizationOptions> configureLocalization)
    {
        app.UseRequiredMiddlewares();
        app.UseRequestLocalization(configureLocalization);
        return app;
    }
    
    public static WebApplication UseJJMasterDataWeb(this WebApplication app, Action<MasterDataLocalizationOptions>? configureLocalization = null)
    {
        app.UseRequiredMiddlewares();

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
            
                var segments = context.Request.Path.Value?.Split(new[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries);
                
                var culturePattern = new Regex("^[a-z]{2}(-[a-z]{2,4})?$",
                    RegexOptions.IgnoreCase);
            
                if (segments?.Length > 0 && culturePattern.IsMatch(segments[0]))
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

    private static void UseRequiredMiddlewares(this WebApplication app)
    {
        app.UseSession();

        //Debug for typescript        
#if DEBUG
        app.UseFileServer(new FileServerOptions
        {
            FileProvider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "Scripts"),
            RequestPath = new PathString("/Scripts"),
        });
#endif
        app.UseStaticFiles();

        app.UseWebOptimizer();
        
        app.UseDefaultFiles();
    }

    /// <summary>
    /// Adds endpoints for JJMasterData Pages to the <see cref="WebApplication"/>.
    /// </summary>
    public static ControllerActionEndpointConventionBuilder MapJJMasterData(this WebApplication app,
        Action<MasterDataAreaOptions>? configure = null)
    {
        var options = new MasterDataAreaOptions();

        configure?.Invoke(options);

        var pattern = "{area}/{controller}/{action}/{elementName?}/";

        if (options.Prefix is not null)
            pattern = $"{options.Prefix.Replace("/", string.Empty)}/{pattern}";

        if (options.EnableCultureProvider)
            pattern = $"/{{culture}}/{pattern}";

        return app.MapControllerRoute(
            name: "JJMasterData",
            pattern: pattern);
    }
}