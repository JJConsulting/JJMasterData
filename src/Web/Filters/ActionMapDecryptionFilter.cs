using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JJMasterData.Web.Filters;


using Microsoft.AspNetCore.Mvc.Filters;

public class ActionMapDecryptionFilter : ActionFilterAttribute
{
    private readonly JJMasterDataEncryptionService _encryptionService;

    public ActionMapDecryptionFilter(JJMasterDataEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.ContainsKey("actionMap"))
        {
            if (context.HttpContext.Request.Query.TryGetValue("actionMap", out var actionMapValue))
            {
                var actionMap = _encryptionService.DecryptActionMap(actionMapValue); 

                context.ActionArguments["actionMap"] = actionMap;
            }
        }
        
    }
}
