using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JJMasterData.Core.Web.Components.Scripts;

internal class DataExpScriptHelper
{
    private JJMasterDataUrlHelper UrlHelper { get; }

    public DataExpScriptHelper(JJMasterDataUrlHelper urlHelper)
    {
        UrlHelper = urlHelper;
    }
    
    public string GetExportModalScript(string componentName, string dictionaryName, bool isExternalRoute)
    {
        if (isExternalRoute)
        {
            var url = UrlHelper.GetUrl("ExportUI","Form",new {dictionaryName});
            return $"JJDataExp.openExportUI('{url}');";
        }

        return $"JJDataExp.openExportUI('{componentName}');";
    }

}