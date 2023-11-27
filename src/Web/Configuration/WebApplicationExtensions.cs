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
    public static WebApplication UseJJMasterDataWeb(this WebApplication app, string defaultCulture = "en-US")
    {
        app.UseSession();

        app.UseRequestLocalization(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("pt-BR"),
                new CultureInfo("zh-CN")
            };

            options.DefaultRequestCulture = new RequestCulture(defaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            options.AddInitialRequestCultureProvider(new CustomRequestCultureProvider(context =>
            {
                string currentCulture;

                var segments = context.Request.Path.Value?.Split(new[] { '/' },
                    StringSplitOptions.RemoveEmptyEntries);
                
                var culturePattern = new Regex(@"^[a-z]{2}(-[a-z]{2,4})?$",
                    RegexOptions.IgnoreCase);

                if (segments?.Length > 0 && culturePattern.IsMatch(segments[0]))
                {
                    currentCulture = segments[0];
                }
                else
                {
                    currentCulture = defaultCulture;
                }

                var requestCulture = new ProviderCultureResult(currentCulture);

                return Task.FromResult(requestCulture)!;
            }));
        });

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

        return app;
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
            pattern = $"/{{culture={options.DefaultCulture}}}/{pattern}";

        return app.MapControllerRoute(
            name: "JJMasterData",
            pattern: pattern);
    }
}