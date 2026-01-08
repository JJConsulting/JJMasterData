using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

internal sealed class DataPanelScripts(DataPanelForm dataPanelForm)
{
    public string GetReloadPanelScript(
        string elementFieldName, 
        string fieldNameWithPrefix)
    {
        var componentName = dataPanelForm.Name;
        var routeContext =
            dataPanelForm.EncryptionService.EncryptObject(RouteContext.FromFormElement(dataPanelForm.FormElement,
                ComponentContext.DataPanelReload));

        //lang=javascript
        return $"DataPanelHelper.reload('{componentName}','{elementFieldName}','{fieldNameWithPrefix}','{routeContext}');";
    }
}