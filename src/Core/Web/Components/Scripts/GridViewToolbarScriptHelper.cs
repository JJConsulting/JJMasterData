using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;

namespace JJMasterData.Core.Web.Components.Scripts;

internal class GridViewToolbarScriptHelper
{
    private GridViewScriptHelper GridViewScriptHelper { get; }

    public GridViewToolbarScriptHelper(GridViewScriptHelper gridViewScriptHelper)
    {
        GridViewScriptHelper = gridViewScriptHelper;
    }
    
    public string GetRefreshScript(JJGridView gridView)
    {
        string name = gridView.Name;
        if (gridView.IsExternalRoute)
        {
            var url = GridViewScriptHelper.GetUrl(gridView.FormElement.Name);
            return $"JJGridView.Refresh('{name}', '{url}')";
        }

        string enableAjax = gridView.EnableAjax ? "true" : "false";
        return $"jjview.doRefresh('{name}', {enableAjax})";
    }

    public  string GetFilterScript(JJGridView gridView)
    {
        string name = gridView.Name;
        if (gridView.IsExternalRoute)
        {
            var url = GridViewScriptHelper.GetUrl(gridView.FormElement.Name);
            return $"JJGridView.Filter('{name}', '{url}')";
        }
        string enableAjax = gridView.EnableAjax ? "true" : "false";
        return $"jjview.doFilter('{name}','{enableAjax}')";
    }


}