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
            return $"DataExportation.startExportationAtSamePage('{componentName}');";

        var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(dictionaryName);
        
        var startExportationUrl = UrlHelper.GetUrl("StartExportation","Exportation","MasterData", new { dictionaryName=encryptedDictionaryName, componentName});
        
        var checkProgressUrl = UrlHelper.GetUrl("CheckProgress","Exportation","MasterData", new { dictionaryName=encryptedDictionaryName, componentName });
        
        return $"DataExportation.startExportation('{startExportationUrl}','{checkProgressUrl}', '{componentName}');";
    }
    
    public string GetStopExportationScript(string dictionaryName,string componentName,string stopMessage, bool isExternalRoute)
    {
        if (!isExternalRoute) 
            return $"DataExportation.stopExportationAtSamePage('{componentName}');";

        var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(dictionaryName);
        
        var stopExportationUrl = UrlHelper.GetUrl("StopExportation","Exportation","MasterData", new { dictionaryName=encryptedDictionaryName, componentName});
        
        return $"DataExportation.stopExportation('{stopExportationUrl}', '{stopMessage}', '{componentName}');";

    }
    
    public string GetExportPopupScript(string dictionaryName,string componentName, bool isExternalRoute)
    {
        if (!isExternalRoute) 
            return $"DataExportation.openExportPopupAtSamePage('{componentName}');";
        
        var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(dictionaryName);
        
        var url = UrlHelper.GetUrl("Settings","Exportation","MasterData", new { dictionaryName=encryptedDictionaryName, componentName});
        
        return $"DataExportation.openExportPopup('{url}', '{componentName}');";
    }
}