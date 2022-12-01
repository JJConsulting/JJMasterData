using JJMasterData.Commons.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Api.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{

    [Route("/Error")]
    public IActionResult HandleError()
    {
        var exceptionHandlerFeature =
            HttpContext.Features.Get<IExceptionHandlerFeature>()!;

        var responseLetter = ExceptionManager.GetResponse(exceptionHandlerFeature.Error);

        return new ObjectResult(responseLetter)
        {
            StatusCode = responseLetter.Status
        };
    }
}
