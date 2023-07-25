using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
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
    public IActionResult GetGridViewTable(FormElement formElement, string componentName)
    {
        var gridView = GridViewFactory.CreateGridView(formElement);
        gridView.Name = componentName;
        gridView.IsExternalRoute = true;
        
        return Content(gridView.GetTableHtml());
    }

}