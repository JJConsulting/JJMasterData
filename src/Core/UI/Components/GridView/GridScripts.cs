using System.Collections.Generic;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Components.Scripts;

public class GridScripts
{
    private readonly JJGridView _gridView;
    private IEncryptionService EncryptionService => _gridView.EncryptionService;

    public GridScripts(JJGridView gridView)
    {
        _gridView = gridView;
    }

    public string GetSortingScript(string fieldName)
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        return $"GridViewHelper.sortGridValues('{_gridView.Name}','{encryptedRouteContext}','{fieldName}')";
    }
    
    public string GetSortMultItemsScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        return $"GridViewHelper.sortMultItems('{_gridView.Name}','{encryptedRouteContext}')";
    }
    
    public string GetPaginationScript(int page)
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        return $"GridViewHelper.paginate('{_gridView.Name}', '{encryptedRouteContext}', {page})";
    }

    private string GetEncryptedRouteContext(ComponentContext componentContext = ComponentContext.GridViewReload)
    {
        var routeContext = RouteContext.FromFormElement(_gridView.FormElement, componentContext);
        var encryptedRouteContext = EncryptionService.EncryptRouteContext(routeContext);
        return encryptedRouteContext;
    }

    public string GetFilterScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        return $"GridViewFilterHelper.filter('{_gridView.Name}','{encryptedRouteContext}')";
    }
    
    public string GetClearFilterScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        return $"GridViewFilterHelper.clearFilter('{_gridView.Name}','{encryptedRouteContext}')";
    }

    public string GetSelectAllScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext(ComponentContext.GridViewSelectAllRows);
        return $"GridViewSelectionHelper.selectAll('{_gridView.Name}','{encryptedRouteContext}')";
    }
    
    public string GetConfigUIScript(ConfigAction action, IDictionary<string, object> formValues)
    {
        var actionMap = new ActionMap(ActionSource.GridToolbar, _gridView.FormElement, formValues, action.Name);
        string encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);

        return $"GridViewHelper.openSettingsModal('{_gridView.Name}','{encryptedActionMap}');";
    }

    public string GetCloseConfigUIScript()
    {
        return $"GridViewHelper.closeSettingsModal('{_gridView.Name}');";
    }


    public string GetRefreshScript()
    {
        return $"GridViewHelper.refresh('{_gridView.Name}','{GetEncryptedRouteContext()}');";
    }

    public object GetReloadFilterScript()
    {
        return $"GridViewFilterHelper.reload('{_gridView.Name}','{_gridView.Filter.Name}','{GetEncryptedRouteContext(ComponentContext.GridViewFilterReload)}')";
    }
}