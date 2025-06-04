using JJMasterData.Commons.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.WebApi.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController(ILogger<ErrorController> logger) : ControllerBase
{
    [Route("/Error")]
    public IActionResult HandleError()
    {
        var exceptionHandlerFeature =
            HttpContext.Features.Get<IExceptionHandlerFeature>()!;

        var error = exceptionHandlerFeature.Error;
        
        var responseLetter = ExceptionManager.GetResponse(error);

        logger.LogCritical(error, "Unexpected error at WebApi.");
        
        return new ObjectResult(responseLetter)
        {
            StatusCode = responseLetter.Status
        };
    }
}
