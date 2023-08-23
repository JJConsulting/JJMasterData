using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Extensions;

namespace JJMasterData.Core.Web.Components.Scripts;

internal class DataPanelScripts
{

    private readonly DataPanelControl _dataPanelControl;
    private JJMasterDataUrlHelper UrlHelper => _dataPanelControl.UrlHelper;
    private IEncryptionService EncryptionService => _dataPanelControl.EncryptionService;
    

    public DataPanelScripts(DataPanelControl dataPanelControl)
    {
        _dataPanelControl = dataPanelControl;
    }

    
    public string GetReloadPanelScript(string fieldName)
    {
        var componentName = _dataPanelControl.Name;
        var pageState = _dataPanelControl.PageState;

        var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(_dataPanelControl.FormElement.Name);
        
        if(!_dataPanelControl.IsExternalRoute)
            return $"DataPanel.ReloadAtSamePage('{componentName}','{fieldName}');";

        return $"DataPanel.Reload('{UrlHelper.GetUrl("ReloadPanel","Form", "MasterData", new {dictionaryName = encryptedDictionaryName, componentName, pageState})}','{componentName}','{fieldName}')";
    }
}