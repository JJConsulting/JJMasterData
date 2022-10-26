using System.Security.Claims;
using JJMasterData.Commons.Util;
using JJMasterData.Core.WebComponents;
using JJMasterData.Web.Controllers;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
public class FormController : MasterDataController
{
    public IActionResult Render(string dictionaryName)
    {
        var form = GetFormView(dictionaryName);
        var aaaa = form.IsExportPost();

        return View(form);
    }
    
    public IActionResult Download(string filePath)
    {
        var file = System.IO.File.Open(Cript.Descript64(filePath), FileMode.Open) as Stream;
        
        return File(file, "application/octet-stream");
    }

    private JJFormView GetFormView(string dictionaryName)
    {
        var userId = HttpContext.GetUserId();
        var form = new JJFormView(dictionaryName);
        if (userId != null)
        {
            form.SetCurrentFilter("USERID", userId);
            form.SetUserValues("USERID", userId);
        }
        
        return form;
    }
}