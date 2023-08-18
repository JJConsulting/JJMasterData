using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class UploadViewController : MasterDataController
{
    private IComponentFactory<JJUploadView> Factory { get; }

    public UploadViewController(IComponentFactory<JJUploadView> factory)
    {
        Factory = factory;
    }

    public async Task<IActionResult> GetUploadView(string componentName)
    {
        var view = Factory.Create();
        view.Name = componentName;

        var result = await view.GetResultAsync();

        if (result.IsActionResult())
            return result.ToActionResult();
        
        return Content(result.Content!);
    }
}