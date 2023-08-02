using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Extensions;


namespace JJMasterData.Core.Web.Components.Scripts;

public class DataExportationScripts
{
    private JJMasterDataUrlHelper UrlHelper { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }

    public DataExportationScripts(JJMasterDataUrlHelper urlHelper, JJMasterDataEncryptionService encryptionService)
    {
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
    }
    public string GetStartExportationScript(string dictionaryName,string componentName, bool isExternalRoute)
    {
        if (!isExternalRoute) 
            return $"JJDataExp.doExport('{componentName}');";

        var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(dictionaryName);
        
        var startExportationUrl = UrlHelper.GetUrl("StartExportation","Exportation",new { dictionaryName=encryptedDictionaryName, componentName});
        
        var checkProgressUrl = UrlHelper.GetUrl("CheckProgress","Exportation",new { dictionaryName=encryptedDictionaryName, componentName });
        
        return $"DataExportation.startExportation('{startExportationUrl}','{checkProgressUrl}', '{componentName}');";
    }
    
    public string GetStopExportationScript(string dictionaryName,string componentName,string stopMessage, bool isExternalRoute)
    {
        if (!isExternalRoute) 
            return $"JJDataExp.stopExportation('{componentName}');";

        var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(dictionaryName);
        
        var stopExportationUrl = UrlHelper.GetUrl("StopExportation","Exportation",new { dictionaryName=encryptedDictionaryName, componentName});
        
        return $"DataExportation.stopExportation('{stopExportationUrl}', '{stopMessage}', '{componentName}');";

    }
    
    public string GetExportPopupScript(string dictionaryName,string componentName, bool isExternalRoute)
    {
        if (!isExternalRoute) 
            return $"JJDataExp.openExportUI('{componentName}');";
        
        var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(dictionaryName);
        
        var url = UrlHelper.GetUrl("Settings","Exportation",new { dictionaryName=encryptedDictionaryName, componentName});
        
        return $"DataExportation.openExportPopup('{url}', '{componentName}');";
    }
}