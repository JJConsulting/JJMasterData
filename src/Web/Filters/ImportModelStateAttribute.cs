using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JJMasterData.Web.Filters;

public sealed class ImportModelStateAttribute : ModelStateFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        var controller = filterContext.Controller as Controller;

        if (controller?.TempData[Key] is string serialisedModelState)
        {
            if (filterContext.Result is ViewResult)
            {
                var modelState = DeserializeModelState(serialisedModelState);
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