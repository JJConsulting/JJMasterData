using JJMasterData.Commons.Cryptography;

namespace JJMasterData.Core.Web.Components.Scripts;

public class DataPanelScriptHelper
{
    private JJMasterDataUrlHelper UrlHelper { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }

    public DataPanelScriptHelper(JJMasterDataUrlHelper urlHelper, JJMasterDataEncryptionService encryptionService)
    {
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
    }

    public string GetReloadPanelScript(string dictionaryName, string fieldName, string componentName, bool isExternalRoute)
    {
        if(!isExternalRoute)
            return $"JJDataPanel.doReload('{componentName}','{fieldName}');";

        return $"DataPanel.Reload('{UrlHelper.GetUrl("ReloadPanel","Form", new {dictionaryName, componentName})}','{componentName}','{fieldName}')";
    }
}