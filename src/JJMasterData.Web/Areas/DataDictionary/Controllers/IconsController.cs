using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class IconsController : DataDictionaryController
{
    public IActionResult Index(string inputId)
    {
        return PartialView("_Icons", new IconViewModel{InputId = inputId});
    }
}