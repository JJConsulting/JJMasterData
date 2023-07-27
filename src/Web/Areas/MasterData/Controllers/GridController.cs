using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components.GridView;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class GridController : MasterDataController
{
    private GridViewFactory GridViewFactory { get; }

    public GridController(GridViewFactory gridViewFactory)
    {
        GridViewFactory = gridViewFactory;
    }
    
        
    [HttpPost]
    [ServiceFilter<FormElementDecryptionFilter>]
    public IActionResult SelectAllRows(FormElement formElement, string componentName)
    {
        var gridView = GridViewFactory.CreateGridView(formElement);
        gridView.Name = componentName;
        gridView.IsExternalRoute = true;

        var selectedRows = gridView.GetEncryptedSelectedRows();
        
        return Json(new {selectedRows});
    }
    
    [HttpPost]
    [ServiceFilter<FormElementDecryptionFilter>]
    public async Task<IActionResult> GetGridViewTable(FormElement formElement, string componentName)
    {
        var gridView = GridViewFactory.CreateGridView(formElement);
        gridView.Name = componentName;
        gridView.IsExternalRoute = true;
        
        return Content(await gridView.GetTableHtmlAsync());
    }

}