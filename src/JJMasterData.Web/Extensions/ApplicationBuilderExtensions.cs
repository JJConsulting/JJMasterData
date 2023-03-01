using JJMasterData.Commons.Extensions;
using Microsoft.AspNetCore.Builder;

namespace JJMasterData.Web.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseJJMasterData(this IApplicationBuilder app)
    {
        app.ApplicationServices.UseJJMasterData();
        return app;
    }
}