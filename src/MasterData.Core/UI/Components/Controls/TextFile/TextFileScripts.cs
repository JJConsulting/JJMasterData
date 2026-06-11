#nullable enable

using System.Web;
using System.Threading.Tasks;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

internal sealed class TextFileScripts(JJTextFile textFile)
{
    public string GetShowScript()
    {
        var title = textFile.FormElementField.Label ?? textFile.StringLocalizer["Manage Files"];

        title = HttpUtility.JavaScriptStringEncode(textFile.StringLocalizer[title]);

        var routeContext = RouteContext.FromFormElement(textFile.FormElement, ComponentContext.TextFileUploadView);
        
        return $"TextFileHelper.showUploadView('{textFile.FieldName}','{title}','{textFile.EncryptionService.EncryptObject(routeContext)}','{textFile.GetDraftInputName()}');";
    }

    public string GetRefreshScript()
    {
        var routeContext = RouteContext.FromFormElement(textFile.FormElement, ComponentContext.TextFileUploadView);

        return $"TextFileHelper.refresh('{textFile.FieldName}','{textFile.EncryptionService.EncryptObject(routeContext)}','{textFile.GetDraftInputName()}')";
    }
    
    public async Task<string> GetRefreshInputsScriptAsync()
    {
        return $"TextFileHelper.refreshInputs('{textFile.Name}','{await textFile.GetPresentationTextAsync()}','{await textFile.GetFileNameAsync()}')";
    }
    
}
