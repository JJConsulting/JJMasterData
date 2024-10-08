#nullable enable

using System.Web;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

internal sealed class TextFileScripts(JJTextFile textFile)
{
    public string GetShowScript()
    {
        var title = textFile.FormElementField.Label ?? textFile.StringLocalizer["Manage Files"];

        title = HttpUtility.JavaScriptStringEncode(textFile.StringLocalizer[title]);

        var routeContext = RouteContext.FromFormElement(textFile.FormElement, ComponentContext.TextFileUploadView);
        
        return $"TextFileHelper.showUploadView('{textFile.FieldName}','{title}','{textFile.EncryptionService.EncryptObject(routeContext)}');";
    }

    public string GetRefreshScript()
    {
        var routeContext = RouteContext.FromFormElement(textFile.FormElement, ComponentContext.TextFileUploadView);

        return $"TextFileHelper.refresh('{textFile.FieldName}','{textFile.EncryptionService.EncryptObject(routeContext)}')";
    }
    
    public string GetRefreshInputsScript()
    {
        return $"TextFileHelper.refreshInputs('{textFile.Name}','{textFile.GetPresentationText()}','{textFile.GetFileName()}')";
    }
    
}