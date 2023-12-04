using System.Reflection;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using JJMasterData.Web.Areas.DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;


public class AboutController : DataDictionaryController
{
    public IActionResult Index()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var model = new AboutViewModel
        {
            ExecutingAssemblyProduct = AboutService.GetAssemblyProduct(executingAssembly),
            ExecutingAssemblyVersion = executingAssembly.GetName()
                .Version!.ToString(),
            ExecutingAssemblyCopyright = AboutService.GetAssemblyCopyright(executingAssembly),
            Dependencies = AboutService.GetJJAssemblies(),
            ExecutingAssemblyLastWriteTime = AboutService.GetAssemblyDate(executingAssembly)
        };

        return PartialView("Index", model);
    }
}