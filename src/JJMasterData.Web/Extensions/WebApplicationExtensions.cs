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
    public static ControllerActionEndpointConventionBuilder UseJJMasterDataWeb(this WebApplication app)
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

        return app.MapControllerRoute(
                name: "JJMasterData",
                pattern: "{culture}/{area:exists}/{controller=Element}/{action=Index}/{dictionaryName?}/")
            .PreBufferRequestStream()
            .BufferResponseStream();
    }
}