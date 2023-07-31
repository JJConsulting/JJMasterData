using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class UploadAreaController : MasterDataController
{
    private IUploadAreaService UploadAreaService { get; }
    private FormFileManagerFactory FormFileManagerFactory { get; }

    public UploadAreaController(IUploadAreaService uploadAreaService, FormFileManagerFactory formFileManagerFactory)
    {
        UploadAreaService = uploadAreaService;
        FormFileManagerFactory = formFileManagerFactory;
    }

    public IActionResult UploadFile(string componentName)
    {
        UploadAreaService.OnFileUploaded += (sender, args) =>
        {

            var manager = FormFileManagerFactory.Create(componentName);
            
            try
            {
                manager.CreateFile(args.File, true);
            }
            catch (Exception ex)
            {
                args.ErrorMessage = ex.Message;
            }
        };
        
        var dto = UploadAreaService.UploadFile();
        return Json(dto);
    }
}