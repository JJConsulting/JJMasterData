using JJMasterData.Core.DataManager.Services;
using JJMasterData.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.WebApi.Controllers;

[Authorize]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("masterApi/{elementName}/{id}/{fieldName}/file")]
public class FileController(ElementFileService service) : ControllerBase
{
    [HttpGet]
    [Route("{fileName}")]
    public async Task<IActionResult> GetFile(string elementName, string fieldName, string id, string fileName)
    {
        var fileStream = await service.GetElementFileAsync(elementName, id, fieldName, fileName);

        if(fileStream == null)
            return NotFound();
        
        return File(fileStream, "application/octet-stream", fileName);
    }
    
    [HttpPost]
    public async Task<IActionResult> PostFile(string elementName, string fieldName, string id, IFormFile file)
    {
        await service.SetElementFileAsync(elementName, fieldName, id, file);

        return Created($"masterApi/{elementName}/{id}/{fieldName}/{file.FileName}", "File successfully created.");
    }
    
    [HttpPatch]
    [Route("{fileName}")]
    public async Task<IActionResult> RenameFile(
        string elementName,
        string fieldName,
        string id,
        string fileName,
        [FromQuery] string newName)
    {
        await service.RenameFileAsync(elementName, fieldName, id, fileName, newName);

        return Ok($"File sucessfuly renamed from {fileName} to {newName}");
    }
    
    [HttpDelete]
    [Route("{fileName}")]
    public async Task<IActionResult> DeleteFile(string elementName, string fieldName, string id, string fileName)
    {
        await service.DeleteFileAsync(elementName, fieldName, id, fileName);

        return Ok("File successfully deleted.");
    }
}