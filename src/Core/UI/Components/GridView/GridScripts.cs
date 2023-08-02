using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components.Scripts;

public class GridScripts
{
    private JJMasterDataEncryptionService EncryptionService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IExpressionsService ExpressionsService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public GridScripts(
        JJMasterDataEncryptionService encryptionService, 
        JJMasterDataUrlHelper urlHelper,
        IExpressionsService expressionsService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
    }

    internal string GetUrl(string dictionaryName)
    {
        string dictionaryNameEncrypted = EncryptionService.EncryptString(dictionaryName);
        return UrlHelper.GetUrl("GetGridViewTable", "Grid", new { dictionaryName = dictionaryNameEncrypted });
    }
    public string GetSortingScript(JJGridView gridView, string fieldName)
    {
        if (gridView.IsExternalRoute)
        {
            var url = GetUrl(gridView.FormElement.Name);
            return $"GridView.sorting('{gridView.Name}','{url}','{fieldName}')";
        }

        string ajax = gridView.EnableAjax ? "true" : "false";
        return $"JJView.sortFormValues('{gridView.Name}','{ajax}','{fieldName}')";
    }

    public string GetPaginationScript(JJGridView gridView, int page)
    {
        string name = gridView.Name;
        if (gridView.IsExternalRoute)
        {
            var url = GetUrl(gridView.FormElement.Name);
            return $"GridView.pagination('{name}', '{url}', {page})";
        }

        string enableAjax = gridView.EnableAjax ? "true" : "false";
        return $"JJView.paginateGrid('{name}', {enableAjax}, {page})";
    }

    public string GetRefreshScript(JJGridView gridView)
    {
        string name = gridView.Name;
        if (gridView.IsExternalRoute)
        {
            var url = GetUrl(gridView.FormElement.Name);
            return $"JJGridView.refresh('{name}', '{url}')";
        }

        string enableAjax = gridView.EnableAjax ? "true" : "false";
        return $"JJView.refresh('{name}', {enableAjax})";
    }

    public string GetFilterScript(JJGridView gridView)
    {
        string name = gridView.Name;
        if (gridView.IsExternalRoute)
        {
            var url = GetUrl(gridView.FormElement.Name);
            return $"GridView.filter('{name}', '{url}')";
        }
        string enableAjax = gridView.EnableAjax ? "true" : "false";
        return $"JJView.filter('{name}','{enableAjax}')";
    }
    
    public string GetSelectAllScript(JJGridView gridView)
    {
        string name = gridView.Name;
        if (gridView.IsExternalRoute)
        {
            var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(gridView.FormElement.Name);
            var url = UrlHelper.GetUrl("SelectAllRows","Grid", new {dictionaryName = encryptedDictionaryName});
            return $"GridView.selectAllRows('{name}', '{url}')";
        }
        string enableAjax = gridView.EnableAjax ? "true" : "false";
        return $"JJView.selectAll('{name}','{enableAjax}')";
    }

    //TODO  
    public string GetUrlRedirectScript(
        FormElement formElement,
        string componmentName,
        string fieldName,
        UrlRedirectAction action, 
        IDictionary<string, dynamic> formValues, 
        PageState pageState,
        ActionSource contextAction)
    {
        var actionMap = new ActionMap(contextAction, formElement, formValues, action.Name)
        {
            FieldName = fieldName
        };
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage ?? string.Empty];
        int popupSize = (int)action.PopupSize;

        var script = new StringBuilder();

        if (contextAction is ActionSource.Field or ActionSource.FormToolbar)
        {
            script.Append("FormView.executeRedirectAction('");
            script.Append(componmentName);
            script.Append("','");
            script.Append(encryptedActionMap);
            script.Append('\'');
            if (!string.IsNullOrEmpty(confirmationMessage))
            {
                script.Append(",'");
                script.Append(confirmationMessage);
                script.Append('\'');
            }

            script.Append(");");
        }
        else
        {
            string url = ExpressionsService.ParseExpression(action.UrlRedirect, pageState, false, formValues);
            string popup = action.UrlAsPopUp ? "true" : "false";
            string popUpTitle = action.PopUpTitle;

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
        }

        return script.ToString();
    }

}