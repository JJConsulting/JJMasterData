using System.Reflection;
using JJMasterData.Core.WebComponents;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
public class AboutController : MasterDataController
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