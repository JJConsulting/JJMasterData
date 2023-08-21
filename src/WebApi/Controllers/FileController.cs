using JJMasterData.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("masterApi/{elementName}/{id}/{fieldName}/file")]
public class FileController : ControllerBase
{
    private readonly FileService _service;
    
    public FileController(FileService service)
    {
        _service = service;
    }
    
    [HttpGet]
    [Route("{fileName}")]
    public async Task<IActionResult> GetFile(string elementName, string id, string fieldName, string fileName)
    {
        var fileStream = await _service.GetDictionaryFileAsync(elementName, id, fieldName, fileName);

        return File(fileStream, "application/octet-stream", fileName);
    }
    
    [HttpPost]
    public async Task<IActionResult> PostFile(string elementName, string fieldName, string id, IFormFile file)
    {
        await _service.SetDictionaryFileAsync(elementName, fieldName, id, file);

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
        await _service.RenameFileAsync(elementName, fieldName, id, fileName, newName);

        return Ok($"File sucessfuly renamed from {fileName} to {newName}");
    }
    
    [HttpDelete]
    [Route("{fileName}")]
    public async Task<IActionResult> DeleteFile(string elementName, string fieldName, string id, string fileName)
    {
        await _service.DeleteFileAsync(elementName, fieldName, id, fileName);

        return Ok("File successfully deleted.");
    }
}