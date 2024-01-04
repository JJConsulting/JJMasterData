using System.Collections.Generic;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
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
    
    public string GetGridSettingsScript(ConfigAction action, IDictionary<string, object> formValues)
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
}