using System.Reflection;
using JJMasterData.Core.Web;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using JJMasterData.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;


public class AboutController : DataDictionaryController
{

    
    public AboutController()
    {

    }

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