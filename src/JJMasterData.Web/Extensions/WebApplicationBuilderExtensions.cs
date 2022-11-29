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
    public static WebApplicationBuilder AddJJMasterDataConfiguration(
        this WebApplicationBuilder builder,
        string settingsPath = "appsettings.json")
    {


        return builder;
    }
}