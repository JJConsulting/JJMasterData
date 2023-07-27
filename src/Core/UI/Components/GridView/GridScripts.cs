using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Extensions;

namespace JJMasterData.Core.Web.Components.Scripts;

public class GridScripts
{
    private JJMasterDataEncryptionService EncryptionService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }

    public GridScripts(JJMasterDataEncryptionService encryptionService, JJMasterDataUrlHelper urlHelper)
    {
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
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
        return $"jjview.doSorting('{gridView.Name}','{ajax}','{fieldName}')";
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
        return $"jjview.doPagination('{name}', {enableAjax}, {page})";
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
        return $"jjview.doRefresh('{name}', {enableAjax})";
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
        return $"jjview.doFilter('{name}','{enableAjax}')";
    }
    
    public string GetSelectAllScript(JJGridView gridView)
    {
        string name = gridView.Name;
        if (gridView.IsExternalRoute)
        {
            var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEncode(gridView.FormElement.Name);
            var url = UrlHelper.GetUrl("SelectAllRows","Grid", new {dictionaryName = encryptedDictionaryName});
            return $"GridView.selectAllRows('{name}', '{url}')";
        }
        string enableAjax = gridView.EnableAjax ? "true" : "false";
        return $"jjview.doSelectAll('{name}','{enableAjax}')";
    }

}