using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

internal class DataPanelScripts(DataPanelControl dataPanelControl)
{
    private IEncryptionService EncryptionService => dataPanelControl.EncryptionService;


    public string GetReloadPanelScript(string fieldName)
    {
        var componentName = dataPanelControl.Name;
        
        var routeContext = EncryptionService.EncryptRouteContext(RouteContext.FromFormElement(dataPanelControl.FormElement,ComponentContext.DataPanelReload));
        
        return $"DataPanelHelper.reload('{componentName}','{fieldName}','{routeContext}');";
    }
}