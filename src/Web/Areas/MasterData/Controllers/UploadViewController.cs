using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class UploadViewController : MasterDataController
{
    private IComponentFactory<JJUploadView> Factory { get; }

    public UploadViewController(IComponentFactory<JJUploadView> factory)
    {
        Factory = factory;
    }

    public IActionResult GetUploadView(string componentName)
    {
        var view = Factory.Create();
        view.Name = componentName;
        return Content(view.GetHtml());
    }
}