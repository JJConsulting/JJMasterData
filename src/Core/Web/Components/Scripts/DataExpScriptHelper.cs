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
    
    public string GetExportPopupScript(string componentName, bool isExternalRoute)
    {
        if (!isExternalRoute) 
            return $"JJDataExp.openExportUI('{componentName}');";
        
        var url = UrlHelper.GetUrl("Settings","Export",new { componentName});
        return $"JJDataExp.openExportPopup('{url}', '{componentName}');";

    }

}