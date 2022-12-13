using JJMasterData.Commons.Util;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
public class FileController : MasterDataController
{
    public IActionResult Download(string filePath)
    {
        var descriptedPath = Cript.Descript64(filePath);
        var file = System.IO.File.Open(descriptedPath, FileMode.Open);
        
        return File(file, "application/octet-stream", Path.GetFileName(descriptedPath));
    }
}