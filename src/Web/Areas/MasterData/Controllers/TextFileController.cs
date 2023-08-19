using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class TextFileController : MasterDataController
{
    private IControlFactory<JJTextFile> TextFileFactory { get; }

    public TextFileController(IControlFactory<JJTextFile> textFileFactory)
    {
        TextFileFactory = textFileFactory;
    }

    [ServiceFilter<FormElementDecryptionFilter>]
    public async Task<IActionResult> GetUploadView(FormElement formElement,string fieldName, string componentName)
    {
        var textFile = TextFileFactory.Create();
        textFile.FormElement = formElement;
        textFile.Name = componentName;
        textFile.IsExternalRoute = true;
        textFile.FormElementField = formElement.Fields[fieldName];
        
        string htmlBuilder = await textFile.GetUploadViewResultAsync();
        return Content(htmlBuilder);
    }
}