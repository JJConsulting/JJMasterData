using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FormController : MasterDataController
{
    private readonly JJMasterDataEncryptionService _encryptionService;
    private readonly JJMasterDataFactory _masterDataFactory;

    public FormController(JJMasterDataEncryptionService encryptionService, JJMasterDataFactory masterDataFactory)
    {
        _encryptionService = encryptionService;
        _masterDataFactory = masterDataFactory;
    }
    
    public IActionResult Render(string dictionaryName)
    {
        var model = new FormViewModel(dictionaryName, ConfigureFormView);
        return View(model);
    }

    private void ConfigureFormView(JJFormView formView)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null) 
            return;
        
        formView.SetCurrentFilter("USERID", userId);
        formView.SetUserValues("USERID", userId);
    }

    [HttpPost]
    public IActionResult GetGrid(string dictionaryNameEncrypted)
    {
        var dictionaryName = _encryptionService.DecryptString(dictionaryNameEncrypted);
        var formView = new JJFormView(dictionaryName);
        return Content(formView.GetTableHtml());
    }
    

    [HttpPost]
    public IActionResult GetSearchValues(string dictionaryName, string fieldName, int pageState)
    {
        var searchBox = _masterDataFactory.CreateJJSearchBox(dictionaryName, fieldName, (PageState)pageState, null);
        var textSearch = HttpContext.Request.Form[$"{searchBox.Name}_text"];
        return Json(searchBox.GetListBoxItems(textSearch));
    }
}