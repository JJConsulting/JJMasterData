using System.Reflection;
using JJMasterData.Web.Areas.DataDictionary.Models;
using JJMasterData.Web.Utils;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;


public class AboutController : DataDictionaryController
{
    public IActionResult Index()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var model = new AboutViewModel
        {
            ExecutingAssemblyProduct = AssemblyHelper.GetAssemblyProduct(executingAssembly),
            ExecutingAssemblyVersion = executingAssembly.GetName()
                .Version!.ToString(),
            ExecutingAssemblyCopyright = AssemblyHelper.GetAssemblyCopyright(executingAssembly),
            ExecutingAssemblyLastWriteTime = AssemblyHelper.GetLastWriteTimeUtc(executingAssembly)
        };

        return PartialView("Index", model);
    }
}