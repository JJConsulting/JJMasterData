using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FileController : MasterDataController
{
    private JJMasterDataEncryptionService EncryptionService { get; }

    public FileController(JJMasterDataEncryptionService encryptionService)
    {
        EncryptionService = encryptionService;
    }
    
    public IActionResult Download(string filePath)
    {
        var descriptedPath = EncryptionService.DecryptStringWithUrlDecode(filePath);
        var file = System.IO.File.Open(descriptedPath, FileMode.Open);
        
        return File(file, "application/octet-stream", Path.GetFileName(descriptedPath));
    }
}