using JJMasterData.Commons.Cryptography;

namespace JJMasterData.Core.Web.Components.Scripts;

internal class GridViewScriptHelper
{
    private JJMasterDataEncryptionService EncryptionService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }

    public GridViewScriptHelper(JJMasterDataEncryptionService encryptionService, JJMasterDataUrlHelper urlHelper)
    {
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
    }
    
    internal string GetUrl(string dictionaryName)
    {
        string dictionaryNameEncrypted = EncryptionService.EncryptString(dictionaryName);
        return UrlHelper.GetUrl("GetGridViewTable", "Form", new { dictionaryName = dictionaryNameEncrypted });
    }
    public string GetSortingScript(JJGridView gridView, string fieldName)
    {
        if (gridView.IsExternalRoute)
        {
            var url = GetUrl(gridView.FormElement.Name);
            return $"JJGridView.Sorting('{gridView.Name}','{url}','{fieldName}')";
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
            return $"JJGridView.Pagination('{name}', '{url}', {page})";
        }

        string enableAjax = gridView.EnableAjax ? "true" : "false";
        return $"jjview.doPagination('{name}', {enableAjax}, {page})";
    }

}