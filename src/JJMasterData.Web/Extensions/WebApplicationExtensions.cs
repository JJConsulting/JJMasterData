using JJMasterData.Commons.Extensions;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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