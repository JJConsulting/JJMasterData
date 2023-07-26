using JJMasterData.Commons.Cryptography;

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
        if(!isExternalRoute)
            return $"JJDataPanel.doReload('{componentName}','{fieldName}');";

        return $"DataPanel.Reload('{UrlHelper.GetUrl("ReloadPanel","Form", new {dictionaryName, componentName})}','{componentName}','{fieldName}')";
    }
}