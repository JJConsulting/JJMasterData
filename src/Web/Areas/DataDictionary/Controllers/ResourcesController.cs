using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class ResourcesController : DataDictionaryController
{
    private readonly ResourcesService _resourcesService;
    private readonly IOptions<JJMasterDataWebOptions> _webOptions;
    private readonly RequestLocalizationOptions _options;
    public ResourcesController(ResourcesService resourcesService, IOptions<RequestLocalizationOptions> options, IOptions<JJMasterDataWebOptions> webOptions)
    {
        _resourcesService = resourcesService;
        _webOptions = webOptions;
        _options = options.Value;
    }

    public ActionResult Index()
    {
        string tablename = _webOptions.Value.ResourcesTableName;
        
        if (string.IsNullOrEmpty(tablename))
        {
            throw new JJMasterDataException("Resources table not found.");
        }

        var formView = _resourcesService.GetFormView(_options.SupportedCultures);
        return View(formView);
    }

}