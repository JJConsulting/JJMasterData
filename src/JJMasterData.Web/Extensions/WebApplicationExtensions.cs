using JJMasterData.Commons.Extensions;
using JJMasterData.Web.Models;
using JJMasterData.Web.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
#if DEBUG
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using System.Reflection;
#endif

namespace JJMasterData.Web.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseJJMasterDataWeb(this WebApplication app)
    {
        app.UseRequestLocalization();
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
        app.UseDefaultFiles();
        app.UseSystemWebAdapters();
        app.UseJJMasterData();

        return app;
    }

    /// <summary>
    /// Adds endpoints for JJMasterData Pages to the <see cref="WebApplication"/>.
    /// </summary>
    public static ControllerActionEndpointConventionBuilder MapJJMasterData(this WebApplication app, Action<JJMasterDataAreaOptions>? configure = null)
    {
        var options = new JJMasterDataAreaOptions();

        configure?.Invoke(options);

        var pattern = "{area:exists}/{controller=Element}/{action=Index}/{dictionaryName?}/";
        
        if (options.Prefix is not null)
            pattern = options.Prefix.Replace("/",string.Empty) + "/" + pattern;
        
        if (options.EnableCultureProvider)
            pattern = "/{culture}/" + pattern;
        
        return app.MapControllerRoute(
            name: "JJMasterData",
            pattern: pattern).BufferResponseStream().PreBufferRequestStream();
    }
}