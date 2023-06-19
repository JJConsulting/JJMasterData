using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FormController : MasterDataController
{
    private readonly JJMasterDataEncryptionService _encryptionService;

    public FormController(JJMasterDataEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
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
    public IActionResult GetGrid(string dictionaryNameEncrypted, int currentPage)
    {
        var dictionaryName = _encryptionService.DecryptString(dictionaryNameEncrypted);
        var formView = new JJFormView(dictionaryName);
        formView.CurrentPage = currentPage;

        return Content(formView.GetTableHtml());
    }
}