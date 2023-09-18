#nullable enable
using System.Collections.Generic;
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
    
    public string GetInsertSelectionScript(IDictionary<string,object?> values)
    {
        var routeContext = RouteContext.FromFormElement(_formView.FormElement, ComponentContext.InsertSelection);

        var encryptedRouteContext = _formView.EncryptionService.EncryptRouteContext(routeContext);

        var encryptedValues = _formView.EncryptionService.EncryptDictionary(values);
        
        return $"FormViewHelper.insertSelection('{_formView.Name}','{encryptedValues}','{encryptedRouteContext}')";
    }
}