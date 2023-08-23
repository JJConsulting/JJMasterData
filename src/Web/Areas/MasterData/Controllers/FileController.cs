using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FileController : MasterDataController
{
    private IEncryptionService EncryptionService { get; }

    public FileController(IEncryptionService encryptionService)
    {
        EncryptionService = encryptionService;
    }
    
    public IActionResult Download(string filePath)
    {
        var descriptedPath = EncryptionService.DecryptStringWithUrlUnescape(filePath);
        var file = System.IO.File.Open(descriptedPath, FileMode.Open);
        
        return File(file, "application/octet-stream", Path.GetFileName(descriptedPath));
    }
}