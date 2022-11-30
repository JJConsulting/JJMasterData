using Microsoft.AspNetCore.Mvc.Filters;

namespace JJMasterData.Web.Filters;

internal interface IHttpRequestAdapterFeature
{
    bool IsEnded { get; }

    bool SuppressContent { get; set; }

    Task EndAsync();

    void ClearContent();
}

// Workaround for https://github.com/dotnet/systemweb-adapters/issues/111 while Microsoft don't update the NuGet package
internal class ResponseEndFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is not null && context.HttpContext.Features.Get<IHttpRequestAdapterFeature>() is { IsEnded: true })
        {
            context.Result = null;
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}