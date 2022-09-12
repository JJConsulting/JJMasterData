using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Web.Filters;

internal interface IHttpRequestAdapterFeature
{
    bool IsEnded { get; }

    bool SuppressContent { get; set; }

    Task EndAsync();

    void ClearContent();
}

// Workaround for https://github.com/dotnet/systemweb-adapters/issues/111 while Microsoft don't update the NuGet package
internal partial class ResponseEndFilter : IActionFilter
{
    private readonly ILogger<ResponseEndFilter> _logger;

    [LoggerMessage(0, LogLevel.Trace, "Clearing MVC result since HttpResponse.End() was called")]
    private partial void LogClearingResult();

    public ResponseEndFilter(ILogger<ResponseEndFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is not null && context.HttpContext.Features.Get<IHttpRequestAdapterFeature>() is { IsEnded: true })
        {
            LogClearingResult();
            context.Result = null;
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}