using JJMasterData.Commons.DI;
using JJMasterData.Commons.Options;
using JJMasterData.Core.Extensions;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Web.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static JJServiceBuilder AddJJMasterDataWeb(this WebApplicationBuilder builder, string settingsPath = "appsettings.json")
    {
        builder.Services.AddOptions<JJMasterDataOptions>();
        builder.Services.ConfigureOptions(typeof(JJMasterDataConfigureOptions));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSession();

        builder.Services.ConfigureOptionsWriter<JJMasterDataOptions>(
            builder.Configuration.GetSection("JJMasterData"), settingsPath);
        
        builder.Services.ConfigureOptionsWriter<ConnectionStrings>(
            builder.Configuration.GetSection("ConnectionStrings"), settingsPath);

        builder.Services.AddSystemWebAdaptersServices();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddJJMasterDataServices();
        builder.Services.AddUrlRequestCultureProvider();
        builder.Services.AddAnonymousAuthorization();

        return builder.Services.AddJJMasterDataCore();
    }
}