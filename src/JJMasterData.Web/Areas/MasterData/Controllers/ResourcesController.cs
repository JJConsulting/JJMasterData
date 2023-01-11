using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Options;
using JJMasterData.Core.DataDictionary.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
public class ResourcesController : MasterDataController
{
    public readonly JJMasterDataCommonsOptions OnMasterDataCommonsOptions;
    private readonly ResourcesService _resourcesService;
    private readonly RequestLocalizationOptions _requestOptions;
    public ResourcesController(ResourcesService resourcesService, IOptions<RequestLocalizationOptions> options, IOptions<JJMasterDataCommonsOptions> masterDataOptions)
    {
        OnMasterDataCommonsOptions = masterDataOptions.Value;
        _resourcesService = resourcesService;
        _requestOptions = options.Value;
    }

    public ActionResult Index()
    {
        string tablename = OnMasterDataCommonsOptions.ResourcesTableName;
        
        if (string.IsNullOrEmpty(tablename))
        {
            throw new JJMasterDataException("Resources table not found.");
        }
        
        var formView = _resourcesService.GetFormView(_requestOptions.SupportedCultures);
        
        return View(formView);
    }

}