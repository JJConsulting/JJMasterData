using JJMasterData.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("masterApi/{elementName}/{id}/{fieldName}/file")]
public class FileController(FileService service) : ControllerBase
{
    [HttpGet]
    [Route("{fileName}")]
    public async Task<IActionResult> GetFile(string elementName, string id, string fieldName, string fileName)
    {
        var fileStream = await service.GetDictionaryFileAsync(elementName, id, fieldName, fileName);

        return File(fileStream, "application/octet-stream", fileName);
    }
    
    [HttpPost]
    public async Task<IActionResult> PostFile(string elementName, string fieldName, string id, IFormFile file)
    {
        await service.SetDictionaryFileAsync(elementName, fieldName, id, file);

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