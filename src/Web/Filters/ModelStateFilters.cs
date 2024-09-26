using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JJMasterData.Web.Filters;

public abstract class ModelStateTransfer : ActionFilterAttribute
{
    protected const string Key = nameof(ModelStateTransfer);
}

public sealed class ExportModelStateAttribute : ModelStateTransfer
{
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        if (!filterContext.ModelState.IsValid)
        {
            if (filterContext.Result is RedirectResult or RedirectToRouteResult or RedirectToActionResult)
            {
                if (filterContext is { Controller: Controller controller })
                {
                    var modelState = ModelStateHelpers.SerialiseModelState(filterContext.ModelState);
                    controller.TempData[Key] = modelState;
                }
            }
        }

        base.OnActionExecuted(filterContext);
    }
}

public sealed class ImportModelStateAttribute : ModelStateTransfer
{
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        var controller = filterContext.Controller as Controller;

        if (controller?.TempData[Key] is string serialisedModelState)
        {
            if (filterContext.Result is ViewResult)
            {
                var modelState = ModelStateHelpers.DeserialiseModelState(serialisedModelState);
                filterContext.ModelState.Merge(modelState);
            }
            else
            {
                controller.TempData.Remove(Key);
            }
        }

        base.OnActionExecuted(filterContext);
    }
}