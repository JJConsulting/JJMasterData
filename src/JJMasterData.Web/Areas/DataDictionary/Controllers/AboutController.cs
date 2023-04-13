using System.Reflection;
using JJMasterData.Core.Web;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using JJMasterData.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;


public class AboutController : DataDictionaryController
{
    private AboutService Service { get; }
    
    public AboutController(AboutService service)
    {
        Service = service;
    }

    public IActionResult Index()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var model = new AboutViewModel
        {
            ExecutingAssemblyProduct = Service.GetAssemblyProduct(executingAssembly),
            ExecutingAssemblyVersion =executingAssembly.GetName().Version?.ToString(),
            ExecutingAssemblyCopyright = Service.GetAssemblyCopyright(executingAssembly),
            BootstrapVersion = BootstrapHelper.Version.ToString(),
            Dependencies = Service.GetJJAssemblies()
        };

        return View("Index", model);
    }
}