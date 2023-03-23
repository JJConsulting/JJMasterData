using JJMasterData.Commons.Localization;
using JJMasterData.WebExample.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace JJMasterData.WebExample.Controllers;

public class ErrorController : Controller
{
    [Route("/Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index([FromQuery]int? statusCode, [FromServices] ILogger<ErrorController>? logger)
    {
        var exceptionHandler =
            HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        
        var model = new ErrorViewModel
        {
            Message = ReasonPhrases.GetReasonPhrase(statusCode ?? 500),
            StatusCode = statusCode ?? 500,
            Exception = exceptionHandler?.Error.Message ?? Translate.Key("Page not found"),
            StackTrace = exceptionHandler?.Error.StackTrace ?? Translate.Key("No stacktrace available.")
        };
        
        logger?.LogError(exceptionHandler?.Error, exceptionHandler?.Error.Message);

        return View(model);
    }
}
