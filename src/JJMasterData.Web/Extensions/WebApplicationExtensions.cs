using System.Reflection;
using JJMasterData.Commons.Extensions;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace JJMasterData.Web.Extensions;

public static class WebApplicationExtensions
{
    public static void UseJJMasterDataWeb(this WebApplication app, Action<JJMasterDataOptions>? configure = null)
    {
        var options = new JJMasterDataOptions();

        configure?.Invoke(options);
        
        app.UseAuthorization();
        app.UseRequestLocalization();
        
        app.UseSession();
        
        app.UseDefaultFiles();
        app.UseStaticFiles();
        
#if DEBUG
        app.UseFileServer(new FileServerOptions
        {
            FileProvider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "Scripts"),
            RequestPath = new PathString("/Scripts"),
        });
#endif

        app.MapControllerRoute(
                 name: "JJMasterData",
                 pattern: "{culture}/{area:exists}/{controller=Element}/{action=Index}/{dictionaryName?}/")
            .PreBufferRequestStream().BufferResponseStream().WithAttributes(options.RouteAttributes?.ToArray());

        app.UseSystemWebAdapters();
        app.UseJJMasterData();
    }
}