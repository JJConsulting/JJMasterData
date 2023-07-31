using JJMasterData.Core.DataManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class UploadAreaController : MasterDataController
{
    private IUploadAreaService UploadAreaService { get; }

    public UploadAreaController(IUploadAreaService uploadAreaService)
    {
        UploadAreaService = uploadAreaService;
    }

    public IActionResult UploadFile()
    {
        var dto = UploadAreaService.UploadFile();
        return Json(dto);
    }
}