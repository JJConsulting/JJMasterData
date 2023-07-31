using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class TextFileController : MasterDataController
{
    private IControlFactory<JJTextFile> TextFileFactory { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }

    public TextFileController(IControlFactory<JJTextFile> textFileFactory, IDataDictionaryRepository dataDictionaryRepository)
    {
        TextFileFactory = textFileFactory;
        DataDictionaryRepository = dataDictionaryRepository;
    }

    [ServiceFilter<FormElementDecryptionFilter>]
    public IActionResult GetUploadView(FormElement formElement,string fieldName, string componentName)
    {
        var textFile = TextFileFactory.Create();
        textFile.FormElement = formElement;
        textFile.Name = componentName;
        textFile.IsExternalRoute = true;
        textFile.FormElementField = formElement.Fields[fieldName];
        
        var htmlBuilder = textFile.GetFormUploadHtmlBuilder();

        return Content(htmlBuilder.ToString());
    }
}