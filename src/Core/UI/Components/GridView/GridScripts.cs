using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

public class GridScripts
{
    private readonly IEncryptionService _encryptionService;
    private readonly JJGridView _gridView;
    private readonly string _defaultEncryptedRouteContext;

    public GridScripts(JJGridView gridView)
    {
        _gridView = gridView;
        _encryptionService = gridView.EncryptionService;
        
        var defaultRouteContext = RouteContext.FromFormElement(_gridView.FormElement, ComponentContext.GridViewReload);
        
        _defaultEncryptedRouteContext = _encryptionService.EncryptObject(defaultRouteContext);
    }

    public string GetSortingScript(string fieldName)
    {
        // language=JavaScript
        return $"GridViewHelper.sortGridValues('{_gridView.Name}','{_defaultEncryptedRouteContext}','{fieldName}')";
    }
    
    public string GetSortMultItemsScript()
    {
        // language=JavaScript
        return $"GridViewHelper.sortMultItems('{_gridView.Name}','{_defaultEncryptedRouteContext}')";
    }
    
    public string GetPaginationScript(int page)
    {
        // language=JavaScript
        return $"GridViewHelper.paginate('{_gridView.Name}', '{_defaultEncryptedRouteContext}', {page})";
    }

    public string GetJumpToPageScript()
    {
        // language=JavaScript
        return $"GridViewHelper.jumpToPage('{_gridView.Name}','{_defaultEncryptedRouteContext}')";
    }

    public string GetFilterScript()
    {
        // language=JavaScript
        return $"GridViewFilterHelper.filter('{_gridView.Name}','{_defaultEncryptedRouteContext}',{_gridView.FilterAction.IsSubmit.ToString().ToLowerInvariant()},)";
    }
    
    public string GetClearFilterScript()
    {
        // language=JavaScript
        return $"GridViewFilterHelper.clearFilter('{_gridView.Name}','{_defaultEncryptedRouteContext}',{_gridView.FilterAction.IsSubmit.ToString().ToLowerInvariant()})";
    }

    public string GetSelectAllScript()
    {
        var encryptedRouteContext = GetEncryptedRouteContext(ComponentContext.GridViewSelectAllRows);
        // language=JavaScript
        return $"GridViewSelectionHelper.selectAll('{_gridView.Name}','{encryptedRouteContext}')";
    }
    
    public string GetGridSettingsScript(ConfigAction action, Dictionary<string, object> formValues)
    {
        var actionMap = new ActionMap(ActionSource.GridToolbar, _gridView.FormElement, formValues, action.Name);
        string encryptedActionMap = _encryptionService.EncryptObject(actionMap);

        // language=JavaScript
        return $"GridViewHelper.setGridSettings('{_gridView.Name}','{_defaultEncryptedRouteContext}','{encryptedActionMap}');";
    }

    public string GetCloseConfigUIScript()
    {
        // language=JavaScript
        return $"GridViewHelper.closeSettingsModal('{_gridView.Name}');";
    }


    public string GetRefreshScript()
    {
        // language=JavaScript
        return $"GridViewHelper.refresh('{_gridView.Name}','{_defaultEncryptedRouteContext}');";
    }

    public object GetReloadFilterScript()
    {
        // language=JavaScript
        return $"setTimeout(()=>GridViewFilterHelper.reload('{_gridView.Name}','{_gridView.Filter.Name}','{GetEncryptedRouteContext(ComponentContext.GridViewFilterReload)}'),200)";
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
        var componentName = _gridView.Name;

        var routeContext = GetEncryptedRouteContext(ComponentContext.GridViewRow);
        
        //language=Javascript
        return $"GridViewHelper.reloadGridRow('{componentName}','{fieldName}',{gridViewRowIndex},'{routeContext}');";
    }
    
    private string GetEncryptedRouteContext(ComponentContext componentContext)
    {
        var routeContext = RouteContext.FromFormElement(_gridView.FormElement, componentContext);
        return _encryptionService.EncryptObject(routeContext);
    }
}