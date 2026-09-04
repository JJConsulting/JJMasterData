using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FileController(
    ElementFileService service,
    IFileStorage fileStorage,
    IDataDictionaryRepository dictionaryRepository,
    IMasterDataUser masterDataUser,
    IOptionsSnapshot<MasterDataCoreOptions> options) : MasterDataController
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

    [ResponseCache(Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Exportation(string elementName, string fileName)
    {
        if (string.IsNullOrWhiteSpace(elementName) || string.IsNullOrWhiteSpace(fileName))
            return BadRequest();

        var formElement = await dictionaryRepository.GetFormElementAsync(elementName);
        var folderPath = DataExportationHelper.GetExportationFolderPath(
            formElement,
            options.Value.ExportationFolderPath,
            masterDataUser.Id);

        var safeFileName = Path.GetFileName(fileName);

        try
        {
            var fullPath = FileStoragePath.Combine(folderPath, safeFileName);
            var stream = await fileStorage.OpenReadAsync(fullPath);
            var contentType = MimeTypeUtil.GetMimeType(safeFileName);
            return File(stream, contentType, safeFileName);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
