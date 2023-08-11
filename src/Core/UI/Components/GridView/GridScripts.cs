using System;
using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components.Scripts;

public class GridScripts
{
    private readonly JJGridView _gridView;
    private JJMasterDataEncryptionService EncryptionService => _gridView.EncryptionService;
    private JJMasterDataUrlHelper UrlHelper => _gridView.UrlHelper;
    private IExpressionsService ExpressionsService => _gridView.ExpressionsService;
    private IStringLocalizer<JJMasterDataResources> StringLocalizer => _gridView.StringLocalizer;

    public GridScripts(JJGridView gridView)
    {
        _gridView = gridView;
    }

    private string GetUrl()
    {
        string dictionaryNameEncrypted = EncryptionService.EncryptStringWithUrlEscape(_gridView.FormElement.Name);
        return UrlHelper.GetUrl("GetGridViewTable", "Grid", "MasterData", new { dictionaryName = dictionaryNameEncrypted});
    }

    public string GetSortingScript(string fieldName)
    {
        if (_gridView.IsExternalRoute)
        {
            var url = GetUrl();
            return $"GridView.sorting('{_gridView.Name}','{url}','{fieldName}')";
        }
        
        return $"JJView.sortFormValues('{_gridView.Name}','{_gridView.EnableAjax.ToString().ToLower()}','{fieldName}')";
    }

    public string GetPaginationScript(int page)
    {
        string name = _gridView.Name;
        if (_gridView.IsExternalRoute)
        {
            var url = GetUrl();
            return $"GridView.pagination('{name}', '{url}', {page})";
        }

        return $"JJView.paginateGrid('{name}', {_gridView.EnableAjax.ToString().ToLower()}, {page})";
    }
    
    public string GetFilterScript()
    {
        string name = _gridView.Name;
        if (_gridView.IsExternalRoute)
        {
            var url = GetUrl();
            return $"GridView.filter('{name}', '{url}')";
        }
        return $"JJView.filter('{name}','{_gridView.EnableAjax.ToString().ToLower()}')";
    }
    
    public string GetClearFilterScript()
    {
        string name = _gridView.Name;
        if (_gridView.IsExternalRoute)
        {
            var url = GetUrl();
            return $"GridView.clearFilter('{name}', '{url}')";
        }
        return $"JJView.clearFilter('{_gridView.Name}','{_gridView.EnableAjax.ToString().ToLower()}')";
    }

    public string GetSelectAllScript()
    {
        string name = _gridView.Name;
        if (_gridView.IsExternalRoute)
        {
            var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(_gridView.FormElement.Name);
            var url = UrlHelper.GetUrl("SelectAllRows", "Grid", "MasterData", new { dictionaryName = encryptedDictionaryName });
            return $"GridView.selectAllRows('{name}', '{url}')";
        }

        return $"JJView.selectAll('{name}','{_gridView.EnableAjax.ToString().ToLower()}')";
    }
    
    public string GetConfigUIScript(ConfigAction action, IDictionary<string, dynamic> formValues)
    {
        var actionMap = new ActionMap(ActionSource.GridToolbar, _gridView.FormElement, formValues, action.Name);
        string encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);

        return $"JJView.openSettingsModal('{_gridView.Name}','{encryptedActionMap}');";
    }

    public string GetCloseConfigUIScript()
    {
        return $"JJView.closeSettingsModal('{_gridView.Name}');";
    }


}