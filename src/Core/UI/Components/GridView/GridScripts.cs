using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

public class GridScripts(JJGridView gridView)
{
    private readonly IEncryptionService _encryptionService = gridView.EncryptionService;
    
    public string GetSortingScript(string fieldName)
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        // language=JavaScript
        return $"GridViewHelper.sortGridValues('{gridView.Name}','{encryptedRouteContext}','{fieldName}')";
    }
    
    public string GetSortMultItemsScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        // language=JavaScript
        return $"GridViewHelper.sortMultItems('{gridView.Name}','{encryptedRouteContext}')";
    }
    
    public string GetPaginationScript(int page)
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        // language=JavaScript
        return $"GridViewHelper.paginate('{gridView.Name}', '{encryptedRouteContext}', {page})";
    }

    public string GetJumpToPageScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        // language=JavaScript
        return $"GridViewHelper.jumpToPage('{gridView.Name}','{encryptedRouteContext}')";
    }
    
    private string GetEncryptedRouteContext(ComponentContext componentContext = ComponentContext.GridViewReload)
    {
        var routeContext = RouteContext.FromFormElement(gridView.FormElement, componentContext);
        var encryptedRouteContext = _encryptionService.EncryptObject(routeContext);
        return encryptedRouteContext;
    }

    public string GetFilterScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        // language=JavaScript
        return $"GridViewFilterHelper.filter('{gridView.Name}','{encryptedRouteContext}')";
    }
    
    public string GetClearFilterScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        // language=JavaScript
        return $"GridViewFilterHelper.clearFilter('{gridView.Name}','{encryptedRouteContext}')";
    }

    public string GetSelectAllScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext(ComponentContext.GridViewSelectAllRows);
        // language=JavaScript
        return $"GridViewSelectionHelper.selectAll('{gridView.Name}','{encryptedRouteContext}')";
    }
    
    public string GetGridSettingsScript(ConfigAction action, Dictionary<string, object> formValues)
    {
        var actionMap = new ActionMap(ActionSource.GridToolbar, gridView.FormElement, formValues, action.Name);
        string encryptedActionMap = _encryptionService.EncryptObject(actionMap);
        var encryptedRouteContext = GetEncryptedRouteContext();
        // language=JavaScript
        return $"GridViewHelper.setGridSettings('{gridView.Name}','{encryptedRouteContext}','{encryptedActionMap}');";
    }

    public string GetCloseConfigUIScript()
    {
        // language=JavaScript
        return $"GridViewHelper.closeSettingsModal('{gridView.Name}');";
    }


    public string GetRefreshScript()
    {
        // language=JavaScript
        return $"GridViewHelper.refresh('{gridView.Name}','{GetEncryptedRouteContext()}');";
    }

    public object GetReloadFilterScript()
    {
        // language=JavaScript
        return $"setTimeout(()=>GridViewFilterHelper.reload('{gridView.Name}','{gridView.Filter.Name}','{GetEncryptedRouteContext(ComponentContext.GridViewFilterReload)}'),200)";
    }

    
    public string GetReloadRowScript(FormElementField field, int gridViewRowIndex)
    {
        var reloadPanelScript = GetReloadRowScript(field.Name,gridViewRowIndex);
        
        //Workaround to trigger event on search component
        if (field.Component is not FormComponent.Search) 
            return reloadPanelScript;
        
        var script = new StringBuilder();

        script.Append("setTimeout(function() { ");
        script.Append(reloadPanelScript);
        script.Append("}, 200);");   
        return script.ToString();

    }
    

    private string GetReloadRowScript(string fieldName, int gridViewRowIndex)
    {
        var componentName = gridView.Name;
        
        var routeContext = _encryptionService.EncryptObject(RouteContext.FromFormElement(gridView.FormElement,ComponentContext.GridViewRow));
        
        //language=Javascript
        return $"GridViewHelper.reloadGridRow('{componentName}','{fieldName}',{gridViewRowIndex},'{routeContext}');";
    }
}