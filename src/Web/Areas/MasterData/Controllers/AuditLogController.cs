using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class AuditLogController : MasterDataController
{
    private IFormElementComponentFactory<JJAuditLogView> Factory { get; }

    public AuditLogController(IFormElementComponentFactory<JJAuditLogView> factory)
    {
        Factory = factory;
    }
    
    [ServiceFilter<FormElementDecryptionFilter>]
    public async Task<IActionResult> GetDetailsPanel(FormElement formElement, string componentName, IFormCollection formCollection)
    {
        var auditLogView = Factory.Create(formElement);

        var panel = await auditLogView.GetDetailsPanelAsync(formCollection[$"logId-{componentName}"]);

        var html = await panel.GetHtmlAsync() ?? string.Empty;
        
        return Content(html);
    }
}