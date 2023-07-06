using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JJMasterData.Web.Filters;


public class LookupParametersDecryptionFilter : ActionFilterAttribute
{
    private readonly JJMasterDataEncryptionService _encryptionService;

    public LookupParametersDecryptionFilter(JJMasterDataEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.ContainsKey("lookupParameters"))
        {
            if (context.HttpContext.Request.Query.TryGetValue("lookupParameters", out var encryptedParameters))
            {
                var decryptedQueryString = _encryptionService.DecryptStringWithUrlDecode(encryptedParameters); 

                context.ActionArguments["lookupParameters"] = LookupParameters.FromQueryString(decryptedQueryString);
            }
        }
        
    }
}
