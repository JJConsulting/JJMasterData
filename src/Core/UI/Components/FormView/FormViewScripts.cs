#nullable enable
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;
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
    
    private string GetEncryptedRouteContext(ComponentContext context)
    {
        var routeContext = RouteContext.FromFormElement(_formView.FormElement, context);
        return _formView.EncryptionService.EncryptRouteContext(routeContext);
    }

    public string GetShowInsertSuccessScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext(ComponentContext.GridViewReload);
        return $"FormViewHelper.showInsertSuccess('{_formView.Name}', '{encryptedRouteContext}')";
    }

    public string GetInsertSelectionScript(IDictionary<string, object?> values)
    {
        var encryptedRouteContext = GetEncryptedRouteContext(ComponentContext.InsertSelection);
        var encryptedValues = _formView.EncryptionService.EncryptDictionary(values);
        return $"FormViewHelper.insertSelection('{_formView.Name}', '{encryptedValues}', '{encryptedRouteContext}')";
    }

    public string GetSetPanelStateScript(PageState pageState)
    {
        var encryptedRouteContext = GetEncryptedRouteContext(ComponentContext.FormViewReload);
        return $"FormViewHelper.setPanelState('{_formView.Name}','{(int)pageState}', '{encryptedRouteContext}')";
    }

}