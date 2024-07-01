#nullable enable
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

public class FormViewScripts(JJFormView formView)
{
    private string GetEncryptedRouteContext(ComponentContext context)
    {
        var routeContext = RouteContext.FromFormElement(formView.FormElement, context);
        return formView.EncryptionService.EncryptObject(routeContext);
    }

    public string GetShowInsertSuccessScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext(ComponentContext.GridViewReload);
        //language=Javascript
        return $"FormViewHelper.showInsertSuccess('{formView.Name}', '{encryptedRouteContext}')";
    }

    public string GetInsertSelectionScript(Dictionary<string, object?> values)
    {
        var encryptedRouteContext = GetEncryptedRouteContext(ComponentContext.InsertSelection);
        var encryptedValues = formView.EncryptionService.EncryptObject(values);
        //language=Javascript
        return $"FormViewHelper.insertSelection('{formView.Name}', '{encryptedValues}', '{encryptedRouteContext}')";
    }

    public string GetSetPageStateScript(PageState pageState)
    {
        var encryptedRouteContext = GetEncryptedRouteContext(ComponentContext.FormViewReload);
        //language=Javascript
        return $"FormViewHelper.setPageState('{formView.Name}','{(int)pageState}', '{encryptedRouteContext}')";
    }
    
    public string GetSetPanelStateScript(PageState pageState)
    {
        var encryptedRouteContext = GetEncryptedRouteContext(ComponentContext.FormViewReload);
        //language=Javascript
        return $"FormViewHelper.setPanelState('{formView.Name}','{formView.DataPanel.Name}','{(int)pageState}', '{encryptedRouteContext}')";
    }

}