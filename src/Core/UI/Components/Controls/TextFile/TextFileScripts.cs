#nullable enable

using System.Web;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.UI.Components.Controls;

internal class TextFileScripts
{
    private readonly JJTextFile _textFile;

    public TextFileScripts(JJTextFile textFile)
    {
        _textFile = textFile;
    }

    public string GetShowScript()
    {
        var title = _textFile.FormElementField.Label ?? "Manage Files";

        title = HttpUtility.JavaScriptStringEncode(_textFile.StringLocalizer[title]);

        var routeContext = RouteContext.FromFormElement(_textFile.FormElement, ComponentContext.TextFileUploadView);
        
        return $"TextFileHelper.showUploadView('{_textFile.FieldName}','{title}','{_textFile.EncryptionService.EncryptRouteContext(routeContext)}');";
    }

    public string GetRefreshScript()
    {
        return $"TextFileHelper.refreshInputs('{_textFile.Name}','{_textFile.GetPresentationText()}','{_textFile.GetFileName()}')";
    }
    
}