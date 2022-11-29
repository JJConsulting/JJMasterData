using JJMasterData.Commons.Extensions;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
#if DEBUG
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using System.Reflection;
#endif

namespace JJMasterData.Web.Extensions;

public static class WebApplicationExtensions
{
    public static void UseJJMasterDataWeb(this WebApplication app, Action<JJMasterDataRoutingOptions>? configure = null)
    {
        var options = new JJMasterDataRoutingOptions();

        configure?.Invoke(options);
        
        app.UseAuthorization();
        app.UseRequestLocalization();
        app.UseSession();
        
#if DEBUG
        app.UseFileServer(new FileServerOptions
        {
            FileProvider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "Scripts"),
            RequestPath = new PathString("/Scripts"),
        });
#endif
        
        app.UseStaticFiles();
        app.UseDefaultFiles();

        app.MapControllerRoute(
                 name: "JJMasterData",
                 pattern: "{culture}/{area:exists}/{controller=Element}/{action=Index}/{dictionaryName?}/")
            .PreBufferRequestStream().BufferResponseStream().WithAttributes(options.RouteAttributes?.ToArray());

        app.UseSystemWebAdapters();
        app.UseJJMasterData();
    }
}