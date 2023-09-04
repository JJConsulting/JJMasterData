using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Extensions;
using Microsoft.Extensions.Primitives;

namespace JJMasterData.Web.Filters;


using Microsoft.AspNetCore.Mvc.Filters;

public class ActionMapDecryptionFilter : ActionFilterAttribute
{
    private readonly IEncryptionService _encryptionService;

    public ActionMapDecryptionFilter(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ActionArguments.ContainsKey("actionMap")) 
            return;
        
        var request = context.HttpContext.Request;
        if (request.Query.TryGetValue("actionMap", out var encryptedQueryActionMap))
        {
            SetActionMap(context, encryptedQueryActionMap);
        }
        else if (request.Query.TryGetValue("componentName", out var componentName))
        {
            if (request.HasFormContentType && request.Form.TryGetValue("form-view-action-map-" + componentName, out var encryptedFormActionMap))
            {
                SetActionMap(context, encryptedFormActionMap);
            }
        }
    }

    private void SetActionMap(ActionExecutingContext context, StringValues encryptedQueryActionMap)
    {
        //TODO: Removing this.
        //var actionMap = _encryptionService.DecryptActionMap(encryptedQueryActionMap);

        //context.ActionArguments["actionMap"] = actionMap;
    }
}
