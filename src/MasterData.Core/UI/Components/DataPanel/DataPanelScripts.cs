using JetBrains.Annotations;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

internal sealed class DataPanelScripts(DataPanelForm dataPanelForm)
{
    private IEncryptionService EncryptionService => dataPanelForm.EncryptionService;

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
        var componentName = dataPanelForm.Name;
        var routeContext =
            EncryptionService.EncryptObject(RouteContext.FromFormElement(dataPanelForm.FormElement,
                ComponentContext.DataPanelReload));


        return $"{methodName}('{componentName}','{elementFieldName}','{fieldNameWithPrefix}','{routeContext}');";
    }
}