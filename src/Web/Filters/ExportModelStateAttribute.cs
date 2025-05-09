using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace JJMasterData.Web.Filters;

public sealed class ExportModelStateAttribute : ModelStateFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        if (!filterContext.ModelState.IsValid)
        {
            if (filterContext.Result is IKeepTempDataResult)
            {
                if (filterContext is { Controller: Controller controller })
                {
                    var modelState = SerialiseModelState(filterContext.ModelState);
                    controller.TempData[Key] = modelState;
                }
            }
        }

        base.OnActionExecuted(filterContext);
    }
}