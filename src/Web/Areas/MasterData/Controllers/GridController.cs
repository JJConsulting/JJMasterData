using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.GridView;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class GridController : MasterDataController
{
    private IFormElementComponentFactory<JJGridView> GridViewFactory { get; }

    public GridController(IFormElementComponentFactory<JJGridView> gridViewFactory)
    {
        GridViewFactory = gridViewFactory;
    }
    
        
    [HttpPost]
    [ServiceFilter<FormElementDecryptionFilter>]
    public async Task<IActionResult> SelectAllRows(FormElement formElement, string componentName)
    {
        var gridView = GridViewFactory.Create(formElement);
        gridView.Name = componentName;
        gridView.IsExternalRoute = true;

        var selectedRows = await gridView.GetEncryptedSelectedRowsAsync();
        
        return Json(new {selectedRows});
    }
    
    [HttpPost]
    [ServiceFilter<FormElementDecryptionFilter>]
    public async Task<IActionResult> GetGridViewTable(FormElement formElement, string componentName)
    {
        var gridView = GridViewFactory.Create(formElement);
        gridView.Name = componentName;
        gridView.IsExternalRoute = true;
        
        return Content(await gridView.GetTableHtmlAsync());
    }
    
    [HttpPost]
    [ServiceFilter<FormElementDecryptionFilter>]
    public async Task<IActionResult> GetTableRowHtml(FormElement formElement, string componentName, int rowIndex)
    {
        var gridView = GridViewFactory.Create(formElement);
        gridView.Name = componentName;
        gridView.IsExternalRoute = true;
        
        return Content(await gridView.GetTableRowHtmlAsync(rowIndex));
    }

}