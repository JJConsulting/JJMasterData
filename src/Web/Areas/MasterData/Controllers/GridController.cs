using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class GridController : MasterDataController
{
    [HttpPost]
    [DictionaryNameDecryptionServiceFilter]
    public IActionResult GetGridViewTable(string dictionaryName)
    {
        var gridView = new JJFormView(dictionaryName);
        return Content(gridView.GetTableHtml());
    }

}