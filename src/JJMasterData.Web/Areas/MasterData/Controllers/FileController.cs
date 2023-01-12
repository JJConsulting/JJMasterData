using JJMasterData.Commons.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
public class FileController : MasterDataController
{
    public JJMasterDataEncryptionService EncryptionService { get; }

    public FileController(JJMasterDataEncryptionService encryptionService)
    {
        EncryptionService = encryptionService;
    }
    
    public IActionResult Download(string filePath)
    {
        var descriptedPath = EncryptionService.DecryptString(filePath);
        var fileStream = System.IO.File.Open(descriptedPath, FileMode.Open);
        return File(fileStream, "application/octet-stream", Path.GetFileName(descriptedPath));
    }
}