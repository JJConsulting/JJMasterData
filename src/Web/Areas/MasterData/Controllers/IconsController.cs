using JJMasterData.Web.Areas.DataDictionary.Models;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class IconsController : MasterDataController
{
    public IActionResult Index(string inputId)
    {
        return PartialView("_Icons", new IconViewModel{InputId = inputId});
    }
}