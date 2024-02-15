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
    private IEncryptionService EncryptionService => gridView.EncryptionService;

    public string GetSortingScript(string fieldName)
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        return $"GridViewHelper.sortGridValues('{gridView.Name}','{encryptedRouteContext}','{fieldName}')";
    }
    
    public string GetSortMultItemsScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        return $"GridViewHelper.sortMultItems('{gridView.Name}','{encryptedRouteContext}')";
    }
    
    public string GetPaginationScript(int page)
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        return $"GridViewHelper.paginate('{gridView.Name}', '{encryptedRouteContext}', {page})";
    }

    public string GetJumpToPageScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        return $"GridViewHelper.jumpToPage('{gridView.Name}','{encryptedRouteContext}')";
    }
    
    private string GetEncryptedRouteContext(ComponentContext componentContext = ComponentContext.GridViewReload)
    {
        var routeContext = RouteContext.FromFormElement(gridView.FormElement, componentContext);
        var encryptedRouteContext = EncryptionService.EncryptRouteContext(routeContext);
        return encryptedRouteContext;
    }

    public string GetFilterScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        return $"GridViewFilterHelper.filter('{gridView.Name}','{encryptedRouteContext}')";
    }
    
    public string GetClearFilterScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        return $"GridViewFilterHelper.clearFilter('{gridView.Name}','{encryptedRouteContext}')";
    }

    public string GetSelectAllScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext(ComponentContext.GridViewSelectAllRows);
        return $"GridViewSelectionHelper.selectAll('{gridView.Name}','{encryptedRouteContext}')";
    }
    
    public string GetGridSettingsScript(ConfigAction action, Dictionary<string, object> formValues)
    {
        var actionMap = new ActionMap(ActionSource.GridToolbar, gridView.FormElement, formValues, action.Name);
        string encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        var encryptedRouteContext = GetEncryptedRouteContext();
        return $"GridViewHelper.setGridSettings('{gridView.Name}','{encryptedRouteContext}','{encryptedActionMap}');";
    }

    public string GetCloseConfigUIScript()
    {
        return $"GridViewHelper.closeSettingsModal('{gridView.Name}');";
    }


    public string GetRefreshScript()
    {
        return $"GridViewHelper.refresh('{gridView.Name}','{GetEncryptedRouteContext()}');";
    }

    public object GetReloadFilterScript()
    {
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
        
        var routeContext = EncryptionService.EncryptRouteContext(RouteContext.FromFormElement(gridView.FormElement,ComponentContext.GridViewRow));
        
        return $"GridViewHelper.reloadGridRow('{componentName}','{fieldName}',{gridViewRowIndex},'{routeContext}');";
    }
}