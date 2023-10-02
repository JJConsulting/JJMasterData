#nullable enable

using System.Web;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

internal class TextFileScripts
{
    private readonly JJTextFile _textFile;

    public TextFileScripts(JJTextFile textFile)
    {
        _textFile = textFile;
    }

    public string GetShowScript()
    {
        var title = _textFile.FormElementField.Label ?? _textFile.StringLocalizer["Manage Files"];

        title = HttpUtility.JavaScriptStringEncode(_textFile.StringLocalizer[title]);

        var routeContext = RouteContext.FromFormElement(_textFile.FormElement, ComponentContext.TextFileUploadView);
        
        return $"TextFileHelper.showUploadView('{_textFile.FieldName}','{title}','{_textFile.EncryptionService.EncryptRouteContext(routeContext)}');";
    }

    public string GetRefreshScript()
    {
        return $"TextFileHelper.refreshInputs('{_textFile.Name}','{_textFile.GetPresentationText()}','{_textFile.GetFileName()}')";
    }
    
}