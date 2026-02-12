using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FileController(ElementFileService service) : MasterDataController
{
    [ResponseCache(Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Index(
        string elementName,
        string fieldName, 
        string id,
        string? fileName = null)
    {
        var fileStream = await service.GetElementFileAsync(elementName, id, fieldName, fileName);

        if(fileStream == null)
            return NotFound();
        
        var extension = !string.IsNullOrWhiteSpace(fileName)
            ? Path.GetExtension(fileName)
            : null;
        
        //fallback
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".bin";
        
        var downloadName = !string.IsNullOrWhiteSpace(fileName)
            ? Path.GetFileName(fileName)
            : $"{DateTime.Now:yyyyMMdd_HHmmss}{extension}";

        var contentType = MimeTypeUtil.GetMimeType(extension);

        return File(fileStream, contentType, downloadName);
    }
}