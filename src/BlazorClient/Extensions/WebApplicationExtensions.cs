using Microsoft.AspNetCore.Builder;

namespace JJMasterData.BlazorClient.Extensions;

public static class WebApplicationExtensions
{
    public static IEndpointConventionBuilder MapJJMasterDataBlazorClient(this WebApplication application)
    {

        return application.MapBlazorHub();
    }
}