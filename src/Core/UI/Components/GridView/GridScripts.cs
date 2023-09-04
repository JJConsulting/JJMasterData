using System;
using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components.Scripts;

public class GridScripts
{
    private readonly JJGridView _gridView;
    private IEncryptionService EncryptionService => _gridView.EncryptionService;
    private IExpressionsService ExpressionsService => _gridView.ExpressionsService;
    private IStringLocalizer<JJMasterDataResources> StringLocalizer => _gridView.StringLocalizer;

    public GridScripts(JJGridView gridView)
    {
        _gridView = gridView;
    }
    

    public string GetSortingScript(string fieldName)
    {
        var encryptedRouteContext = GetEncryptedRouteContext();
        
        return $"GridViewHelper.sortGridValues('{_gridView.Name}','{encryptedRouteContext}','{fieldName}')";
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
        return $"GridViewHelper.selectAll('{_gridView.Name}','{encryptedRouteContext}')";
    }
    
    public string GetConfigUIScript(ConfigAction action, IDictionary<string, object> formValues)
    {
        var actionMap = new ActionMap(ActionSource.GridToolbar, _gridView.FormElement, formValues, action.Name);
        string encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);

        return $"JJViewHelper.openSettingsModal('{_gridView.Name}','{encryptedActionMap}');";
    }

    public string GetCloseConfigUIScript()
    {
        return $"JJViewHelper.closeSettingsModal('{_gridView.Name}');";
    }


}