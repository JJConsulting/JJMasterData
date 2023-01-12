using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
public class ResourcesController : MasterDataController
{
    private readonly ResourcesService _resourcesService;
    private readonly RequestLocalizationOptions _options;
    public ResourcesController(ResourcesService resourcesService, IOptions<RequestLocalizationOptions> options)
    {
        _resourcesService = resourcesService;
        _options = options.Value;
    }

    public ActionResult Index()
    {
        string tablename = JJService.CommonsOptions.ResourcesTableName;
        
        if (string.IsNullOrEmpty(tablename))
        {
            throw new JJMasterDataException("Resources table not found.");
        }
        
        var formView = _resourcesService.GetFormView(_options.SupportedCultures);
        
        return View(formView);
    }

}