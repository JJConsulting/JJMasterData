using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace JJMasterData.Web.Controllers;

public class ErrorController : Controller
{
    [Route("/Error")]

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index([FromQuery]int? statusCode)
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
        

        Log.AddError(model.Exception + "\n\n" + model.StackTrace);

        return View(model);
    }
}
