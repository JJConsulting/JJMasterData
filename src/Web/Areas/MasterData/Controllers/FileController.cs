using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FileController(IEncryptionService encryptionService) : MasterDataController
{
    private IEncryptionService EncryptionService { get; } = encryptionService;

    public IActionResult Download(string filePath)
    {
        var descriptedPath = EncryptionService.DecryptStringWithUrlUnescape(filePath);
        var file = System.IO.File.Open(descriptedPath, FileMode.Open);
        
        return File(file, "application/octet-stream", Path.GetFileName(descriptedPath));
    }
}