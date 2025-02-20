using JJMasterData.Core.DataManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;


public class FileController(ElementFileService service) : MasterDataController
{
    [HttpGet]
    [Route("MasterData/[controller]/{elementName}/{fieldName}/{id}/{fileName}")]
    [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 14400)]
    public async Task<IActionResult> Index(
        [FromRoute] string elementName,
        [FromRoute] string fieldName, 
        [FromRoute] string id,
        [FromRoute] string fileName)
    {
        var fileStream = await service.GetElementFileAsync(elementName, id, fieldName, fileName);

        if(fileStream == null)
            return NotFound();
        
        return File(fileStream, "application/octet-stream", fileName);
    }
}