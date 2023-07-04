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
    [DictionaryNameDecryptionServiceFilter]
    public async Task<IActionResult> GetGridViewTable(string dictionaryName)
    {
        var gridView = await GridViewFactory.CreateGridViewAsync(dictionaryName);
        return Content(gridView.GetTableHtml());
    }

}