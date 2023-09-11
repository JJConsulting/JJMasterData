#nullable enable

using System.Web;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Components;
using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components.Controls;

internal class TextFileScripts
{
    public JJTextFile TextFile { get; }

    public TextFileScripts(JJTextFile textFile)
    {
        TextFile = textFile;
    }

    public string GetShowScript()
    {
        var title = TextFile.FormElementField.Label?? "Manage Files";

        title = HttpUtility.JavaScriptStringEncode(TextFile.StringLocalizer[title]);


        var routeContext = RouteContext.FromFormElement(TextFile.FormElement, ComponentContext.TextFile);
        
        return $"UploadViewHelper.show('{TextFile.Name}','{TextFile.FieldName}','{title}','{TextFile.EncryptionService.EncryptRouteContext(routeContext)}');";
    }

    public string GetRefreshScript(JJUploadView uploadView)
    {
        return $$"""
                         document.addEventListener('DOMContentLoaded', function () {
                             var parentElement = window.parent.document.getElementById('v_{{uploadView.Name}}');
                             var nameElement = window.parent.document.getElementById(uploadView.Name);
                             
                             if (parentElement) {
                                 parentElement.value = '{{TextFile.GetPresentationText(uploadView)}}';
                             }
                             
                             if (nameElement) {
                                 nameElement.value = '{{JJTextFile.GetFileName(uploadView)}}';
                             }
                         });
                   
                 """;
    }
    
}