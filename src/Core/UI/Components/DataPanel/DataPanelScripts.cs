using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;

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
        
        var routeContext = EncryptionService.EncryptRouteContext(RouteContext.FromFormElement(_dataPanelControl.FormElement,ComponentContext.DataPanelReload));
        
        return $"DataPanelHelper.reload('{componentName}','{fieldName}','{routeContext}');";
    }
}