namespace JJMasterData.Core.Web.Components.Scripts;

internal static class GridViewScripts
{

    public static string GetSortingScript(this JJGridView gridView, string fieldName)
    {
        if (gridView.IsExternalRoute)
        {
            var url = ScriptHelper.GetUrl(gridView.FormElement.Name);
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
            var url = ScriptHelper.GetUrl(gridView.FormElement.Name);
            return $"JJGridView.Pagination('{name}', '{url}', {page})";
        }

        string enableAjax = gridView.EnableAjax ? "true" : "false";
        return $"jjview.doPagination('{name}', {enableAjax}, {page})";
    }

}