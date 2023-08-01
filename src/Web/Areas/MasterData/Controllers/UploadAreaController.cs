using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class UploadAreaController : MasterDataController
{
    private IUploadAreaService UploadAreaService { get; }
    private FormFileManagerFactory FormFileManagerFactory { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public UploadAreaController(IUploadAreaService uploadAreaService, FormFileManagerFactory formFileManagerFactory, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        UploadAreaService = uploadAreaService;
        FormFileManagerFactory = formFileManagerFactory;
        StringLocalizer = stringLocalizer;
    }

    public IActionResult UploadFile(string componentName)
    {
        UploadAreaService.OnFileUploaded += (_, args) =>
        {

            var manager = FormFileManagerFactory.Create(componentName);
            
            try
            {
                manager.CreateFile(args.File, true);
                args.SuccessMessage = StringLocalizer["File successfully uploaded."];
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