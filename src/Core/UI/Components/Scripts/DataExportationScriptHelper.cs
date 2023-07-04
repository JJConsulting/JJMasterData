using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JJMasterData.Core.Web.Components.Scripts;

internal class DataExportationScriptHelper
{
    private JJMasterDataUrlHelper UrlHelper { get; }

    public DataExportationScriptHelper(JJMasterDataUrlHelper urlHelper)
    {
        UrlHelper = urlHelper;
    }
    public string GetStartExportationScript(string dictionaryName,string componentName, bool isExternalRoute)
    {
        if (!isExternalRoute) 
            return $"JJDataExp.doExport('{componentName}');";
        
        var url = UrlHelper.GetUrl("Settings","Export",new { dictionaryName});
        return $"DataExportation.startExportation('{url}', '{dictionaryName}');";

    }
    
    public string GetExportPopupScript(string dictionaryName,string componentName, bool isExternalRoute)
    {
        if (!isExternalRoute) 
            return $"JJDataExp.openExportUI('{componentName}');";
        
        var url = UrlHelper.GetUrl("Settings","Export",new { dictionaryName});
        return $"DataExportation.openExportPopup('{url}', '{dictionaryName}');";

    }

}