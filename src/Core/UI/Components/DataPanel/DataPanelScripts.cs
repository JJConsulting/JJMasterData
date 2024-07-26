using JetBrains.Annotations;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

internal sealed class DataPanelScripts(DataPanelControl dataPanelControl)
{
    private IEncryptionService EncryptionService => dataPanelControl.EncryptionService;

    public string GetReloadPanelScript(string elementFieldName, string fieldNameWithPrefix)
    {
        return GetReloadPanelScriptInternal(elementFieldName, fieldNameWithPrefix, "DataPanelHelper.reload");
    }

    public string GetReloadPanelWithTimeoutScript(string elementFieldName, string fieldNameWithPrefix)
    {
        return GetReloadPanelScriptInternal(elementFieldName, fieldNameWithPrefix, "DataPanelHelper.reloadWithTimeout");
    }

    private string GetReloadPanelScriptInternal(
        string elementFieldName, 
        string fieldNameWithPrefix,
        [LanguageInjection("javascript")] string methodName)
    {
        var componentName = dataPanelControl.Name;
        var routeContext =
            EncryptionService.EncryptObject(RouteContext.FromFormElement(dataPanelControl.FormElement,
                ComponentContext.DataPanelReload));


        return $"{methodName}('{componentName}','{elementFieldName}','{fieldNameWithPrefix}','{routeContext}');";
    }
}