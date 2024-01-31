using JJMasterData.Commons.Localization;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Web.Controllers;

public class ErrorController : Controller
{
    [Route("/Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index([FromQuery]int? statusCode, [FromServices] ILogger<ErrorController>? logger, [FromServices] IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        var exceptionHandler =
            HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        
        var model = new ErrorViewModel
        {
            Message = ReasonPhrases.GetReasonPhrase(statusCode ?? 500),
            StatusCode = statusCode ?? 500,
            Exception = exceptionHandler?.Error.Message ?? stringLocalizer["Page not found"]
        };
        
        if(exceptionHandler?.Error is not null)
            logger?.LogCritical(exceptionHandler.Error, "Unhandled exception captured by ErrorController");

        return View(model);
    }
}
