using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace JJMasterData.Core.Web.Components.Scripts;

internal static class DataExpScripts
{

    public static string GetExportModalScript(this JJGridView gridView, int page)
    {
        string name = gridView.Name;
        if (gridView.IsExternalRoute)
        {
            var url = ScriptHelper.GetUrl(gridView.FormElement.Name);
            //return $"JJGridView.Pagination('{name}', '{url}', {page})";
            //TODO
            return $"JJDataExp.openExportUI('{gridView.Name}');";
        }

        return $"JJDataExp.openExportUI('{gridView.Name}');";
    }

}