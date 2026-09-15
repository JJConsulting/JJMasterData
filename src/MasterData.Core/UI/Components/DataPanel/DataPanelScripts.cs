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
            dataPanelForm.DataProtectionService.ProtectObject(RouteContext.FromFormElement(dataPanelForm.FormElement,
                ComponentContext.DataPanelReload));

        //lang=javascript
        return $"DataPanelHelper.reload('{componentName}','{elementFieldName}','{fieldNameWithPrefix}','{routeContext}');";
    }
}