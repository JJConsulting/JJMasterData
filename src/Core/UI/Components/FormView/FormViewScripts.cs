using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.UI.Components.FormView;

public class FormViewScripts
{
    private readonly JJFormView _formView;

    public FormViewScripts(JJFormView formView)
    {
        _formView = formView;
    }
    
    public string GetShowInsertSuccessScript()
    {
        var routeContext = RouteContext.FromFormElement(_formView.FormElement, ComponentContext.GridViewReload);

        var encryptedRouteContext = _formView.EncryptionService.EncryptRouteContext(routeContext);

        return $"FormViewHelper.showInsertSuccess('{_formView.Name}','{encryptedRouteContext}')";
    }
}