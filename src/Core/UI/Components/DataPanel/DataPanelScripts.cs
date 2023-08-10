using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Extensions;

namespace JJMasterData.Core.Web.Components.Scripts;

public class DataPanelScripts
{
    private JJMasterDataUrlHelper UrlHelper { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }

    public DataPanelScripts(JJMasterDataEncryptionService encryptionService, JJMasterDataUrlHelper urlHelper)
    {
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
    }

    public string GetReloadPanelScript(string dictionaryName, string fieldName, string componentName, bool isExternalRoute)
    {
        var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(dictionaryName);
        
        if(!isExternalRoute)
            return $"DataPanel.ReloadAtSamePage('{componentName}','{fieldName}');";

        return $"DataPanel.Reload('{UrlHelper.GetUrl("ReloadPanel","Form", "MasterData", new {dictionaryName = encryptedDictionaryName, componentName})}','{componentName}','{fieldName}')";
    }
}