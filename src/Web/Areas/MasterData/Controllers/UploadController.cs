using JJMasterData.Core.DataManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class UploadController : MasterDataController
{
    private IUploadAreaService UploadAreaService { get; }

    public UploadController(IUploadAreaService uploadAreaService)
    {
        UploadAreaService = uploadAreaService;
    }

    public IActionResult UploadFile()
    {
        var dto = UploadAreaService.UploadFile();
        return Json(dto);
    }
}