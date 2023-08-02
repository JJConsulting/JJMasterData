using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class UrlRedirectController : MasterDataController
{
    private IUrlRedirectService UrlRedirectService { get; }

    public UrlRedirectController(IUrlRedirectService urlRedirectService)
    {
        UrlRedirectService = urlRedirectService;
    }
    
    [ServiceFilter<FormElementDecryptionFilter>]
    [ServiceFilter<ActionMapDecryptionFilter>]
    public async Task<IActionResult> GetUrlRedirect(FormElement formElement,ActionMap actionMap, PageState pageState)
    {
        var model = await UrlRedirectService.GetUrlRedirectAsync(formElement, actionMap, pageState);
        return Json(model);
    }
}