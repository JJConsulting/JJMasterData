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
        string dictionaryNameEncrypted = EncryptionService.EncryptString(_gridView.FormElement.Name);
        return UrlHelper.GetUrl("GetGridViewTable", "Grid", new { dictionaryName = dictionaryNameEncrypted });
    }

    public string GetSortingScript(string fieldName)
    {
        if (_gridView.IsExternalRoute)
        {
            var url = GetUrl();
            return $"GridView.sorting('{_gridView.Name}','{url}','{fieldName}')";
        }

        string ajax = _gridView.EnableAjax ? "true" : "false";
        return $"JJView.sortFormValues('{_gridView.Name}','{ajax}','{fieldName}')";
    }

    public string GetPaginationScript(int page)
    {
        string name = _gridView.Name;
        if (_gridView.IsExternalRoute)
        {
            var url = GetUrl();
            return $"GridView.pagination('{name}', '{url}', {page})";
        }

        string enableAjax = _gridView.EnableAjax ? "true" : "false";
        return $"JJView.paginateGrid('{name}', {enableAjax}, {page})";
    }

    public string GetRefreshScript()
    {
        string name = _gridView.Name;
        if (_gridView.IsExternalRoute)
        {
            var url = GetUrl();
            return $"JJGridView.refresh('{name}', '{url}')";
        }

        string enableAjax = _gridView.EnableAjax ? "true" : "false";
        return $"JJView.refresh('{name}', {enableAjax})";
    }

    public string GetFilterScript()
    {
        string name = _gridView.Name;
        if (_gridView.IsExternalRoute)
        {
            var url = GetUrl();
            return $"GridView.filter('{name}', '{url}')";
        }
        string enableAjax = _gridView.EnableAjax ? "true" : "false";
        return $"JJView.filter('{name}','{enableAjax}')";
    }

    public string GetSelectAllScript()
    {
        string name = _gridView.Name;
        if (_gridView.IsExternalRoute)
        {
            var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(_gridView.FormElement.Name);
            var url = UrlHelper.GetUrl("SelectAllRows", "Grid", new { dictionaryName = encryptedDictionaryName });
            return $"GridView.selectAllRows('{name}', '{url}')";
        }
        string enableAjax = _gridView.EnableAjax ? "true" : "false";
        return $"JJView.selectAll('{name}','{enableAjax}')";
    }


    public string GetUrlRedirectToolbarScript(
        UrlRedirectAction action,
        IDictionary<string, dynamic> formValues)
    {
        int popupSize = (int)action.PopupSize;
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage ?? string.Empty];
        string url = ExpressionsService.ParseExpression(action.UrlRedirect, PageState.List, false, formValues);
        string popup = action.UrlAsPopUp ? "true" : "false";
        string popUpTitle = action.PopUpTitle;

        var script = new StringBuilder();
        script.Append("FormView.doUrlRedirect('");
        script.Append(url);
        script.Append("',");
        script.Append(popup);
        script.Append(",'");
        script.Append(popUpTitle);
        script.Append("','");
        script.Append(confirmationMessage);
        script.Append("','");
        script.Append(popupSize);
        script.Append("');");


        return script.ToString();
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