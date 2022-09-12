using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
public class ResourcesController : MasterDataController
{
    private readonly ResourcesService _resourcesService;
    public ResourcesController(ResourcesService resourcesService)
    {
        _resourcesService = resourcesService;
    }

    public ActionResult Index()
    {
        string tablename = JJService.Settings.ResourcesTableName;
        
        if (string.IsNullOrEmpty(tablename))
        {
            throw new Exception("Resources table not found.");
        }
        
        var formView = _resourcesService.GetFormView();
        
        return View(formView);
    }

}