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
        var folderPath = DataExportationHelper.GetFolderPath(
            formElement,
            options.Value.ExportationFolderPath,
            masterDataUser.Id);

        var safeFileName = Path.GetFileName(fileName);
        var fullFolderPath = Path.GetFullPath(folderPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var filePath = Path.GetFullPath(Path.Combine(fullFolderPath, safeFileName));

        if (!filePath.StartsWith(fullFolderPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || !System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var contentType = MimeTypeUtil.GetMimeType(safeFileName);
        return PhysicalFile(filePath, contentType, safeFileName);
    }
}
