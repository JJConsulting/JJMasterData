using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FileController(ElementFileService service) : MasterDataController
{
    [ResponseCache(Duration = 14400)]
    public async Task<IActionResult> Index(
        string elementName,
        string fieldName, 
        string id,
        string? fileName = null)
    {
        var fileStream = await service.GetElementFileAsync(elementName, id, fieldName, fileName);

        if(fileStream == null)
            return NotFound();
        
        return File(fileStream, MimeTypeUtil.GetMimeType(Path.GetExtension(fileName)), fileName);
    }
}