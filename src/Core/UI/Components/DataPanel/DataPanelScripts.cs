using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

internal class DataPanelScripts(DataPanelControl dataPanelControl)
{
    private IEncryptionService EncryptionService => dataPanelControl.EncryptionService;


    public string GetReloadPanelScript(string elementFieldName, string fieldNameWithPrefix)
    {
        var componentName = dataPanelControl.Name;
        
        var routeContext = EncryptionService.EncryptRouteContext(RouteContext.FromFormElement(dataPanelControl.FormElement,ComponentContext.DataPanelReload));
        
        //language=Javascript
        return $"DataPanelHelper.reload('{componentName}','{elementFieldName}','{fieldNameWithPrefix}','{routeContext}');";
    }
}