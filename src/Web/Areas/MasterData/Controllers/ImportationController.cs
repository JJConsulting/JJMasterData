using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class ImportationController : MasterDataController
{
    private IFormElementComponentFactory<JJDataImportation> Factory { get; }

    public ImportationController(IFormElementComponentFactory<JJDataImportation> factory)
    {
        Factory = factory;
    }
    
    [ServiceFilter<FormElementDecryptionFilter>]
    public IActionResult CheckProgress(FormElement formElement)
    {
        var importation = Factory.Create(formElement);

        var progress = importation.GetCurrentProgress();

        return Json(progress);
    }
    
    [ServiceFilter<FormElementDecryptionFilter>]
    public IActionResult StopProcess(FormElement formElement)
    {
        var importation = Factory.Create(formElement);

        importation.StopExportation();

        return Json(new {isProcessing =  false});
    }
}