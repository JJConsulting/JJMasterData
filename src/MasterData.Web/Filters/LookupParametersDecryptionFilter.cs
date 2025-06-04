using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace JJMasterData.Web.Filters;


public class LookupParametersDecryptionFilter(IEncryptionService encryptionService) : ActionFilterAttribute
{
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
        var lookupQueryString = encryptionService.DecryptStringWithUrlUnescape(encryptedParameters);
        context.ActionArguments["lookupParameters"] = LookupParameters.FromQueryString(lookupQueryString);
    }
}
