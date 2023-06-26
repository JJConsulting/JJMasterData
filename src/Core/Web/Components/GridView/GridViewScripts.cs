using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataManager;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Web.Components.GridView;

internal static class GridViewScripts
{

    public static string GetSortingScript(this JJGridView gridView, string fieldName)
    {
        if (gridView.IsExternalRoute)
        {
            var url = GetUrl(gridView.FormElement.Name);
            return $"JJGridView.Sorting('{gridView.Name}','{url}','{fieldName}')";
        }

        string ajax = gridView.EnableAjax ? "true" : "false";
        return $"jjview.doSorting('{gridView.Name}','{ajax}','{fieldName}')";
    }
    
    public static string GetPaginationScript(this JJGridView gridView, int page)
    {
        string name = gridView.Name;
        if (gridView.IsExternalRoute)
        {
            var url = GetUrl(gridView.FormElement.Name);
            return $"JJGridView.Pagination('{name}', '{url}', {page})";
        }

        string enableAjax = gridView.EnableAjax ? "true" : "false";
        return $"jjview.doPagination('{name}', {enableAjax}, {page})";
    }

    public static string GetFilterScript(this JJGridView gridView)
    {
        string name = gridView.Name;
        if (gridView.IsExternalRoute)
        {
            var url = GetUrl(gridView.FormElement.Name);
            return $"JJGridView.Filter('{name}', '{url}')";
        }

        string enableAjax = gridView.EnableAjax ? "true" : "false";
        return $"jjview.doFilter('{name}','{enableAjax}')";
    }
    
    private static string GetUrl(string dictionaryName)
    {
        var encryptionService = JJService.Provider.GetService<JJMasterDataEncryptionService>();
        var urlHelper = JJMasterDataUrlHelper.GetInstance();
        string dictionaryNameEncrypted = encryptionService.EncryptString(dictionaryName);
        return urlHelper.GetUrl("GetGrid", "Form",new {Area = "MasterData",dictionaryNameEncrypted});
    }
}