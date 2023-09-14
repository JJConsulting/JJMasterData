using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace JJMasterData.Web.Filters;


public class LookupParametersDecryptionFilter : ActionFilterAttribute
{
    private readonly IEncryptionService _encryptionService;

    public LookupParametersDecryptionFilter(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.ContainsKey("lookupParameters"))
        {
            if (context.HttpContext.Request.Query.TryGetValue("lookupParameters", out var encryptedParameters))
            {
                SetLookupParameters(context, encryptedParameters);
            }
            else if (context.HttpContext.Request.HasFormContentType && context.HttpContext.Request.Form.TryGetValue("lookupParameters", out encryptedParameters))
            {
                SetLookupParameters(context, encryptedParameters);
            }
        }
    }

    private void SetLookupParameters(ActionExecutingContext context, StringValues encryptedParameters)
    {
        var lookupQueryString = _encryptionService.DecryptStringWithUrlUnescape(encryptedParameters);
        context.ActionArguments["lookupParameters"] = LookupParameters.FromQueryString(lookupQueryString);
    }
}
