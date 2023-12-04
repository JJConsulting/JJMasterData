using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class IndexesController(IndexesService indexesService) : DataDictionaryController
{
    [ViewData] 
    public string ElementName { get; set; } = null!;
    
    [ViewData] 
    public string MenuId { get; set; } = null!;

    [ViewData] 
    public string IndexName { get; set; } = null!;
    
    [ViewData] 
    public List<SelectListItem> CheckboxList { get; set; } = null!;

    public async Task<ActionResult> Index(string elementName)
    {
        List<ElementIndex> listIndexes = (await indexesService.GetFormElementAsync(elementName)).Indexes;
        PopulateViewData(elementName);

        return View(listIndexes);
    }

    public async Task<ActionResult> Detail(string elementName, string index)
    {
        FormElement formElement = await indexesService.GetFormElementAsync(elementName);

        ElementIndex elementIndex;
        if (!string.IsNullOrEmpty(index))
        {
            elementIndex = formElement.Indexes[int.Parse(index)];
            IndexName = $"IX_{formElement.Name}_{int.Parse(index) + 1}";
        }
        else
        {
            elementIndex = new ElementIndex();
            IndexName = $"IX_{formElement.Name}_{formElement.Indexes.Count}";
        }

        await PopulateCheckBoxAsync(elementName, index);
        PopulateViewData(elementName);
        return View("Detail",  elementIndex);

    }

    [HttpPost]
    public async Task<ActionResult> Detail(string elementName, string? index, List<SelectListItem> checkBoxList, ElementIndex elementIndex)
    {
        List<string> indexColumns = (from item in checkBoxList where item.Selected select item.Value).ToList();

        elementIndex.Columns = indexColumns;

        if (await indexesService.SaveAsync(elementName, index, elementIndex))
        {
            return Json(new { success = true });
        }

        var summary = indexesService.GetValidationSummary();
        return Json(new { success = false, errorMessage = summary.GetHtml() });
    }

    [HttpPost]
    public async Task<ActionResult> Index(string elementName, string filter)
    {
        List<ElementIndex> indexes = (await indexesService.GetFormElementAsync(elementName)).Indexes;

        if (!string.IsNullOrEmpty(filter))
            indexes = indexes.FindAll(l => l.Columns.Contains(filter));


        PopulateViewData(elementName);
        return View(indexes);
    }

    [HttpPost]
    public async Task<ActionResult> Delete(string elementName, string index)
    {
        await indexesService.DeleteAsync(elementName, index);
        return RedirectToAction("Index", new { elementName });
    }

    [HttpPost]
    public async Task<ActionResult> MoveDown(string elementName, string index)
    {
        await indexesService.MoveDownAsync(elementName, index);
        return RedirectToAction("Index", new { elementName });

    }

    [HttpPost]
    public async Task<ActionResult> MoveUp(string elementName, string index)
    {
        await indexesService.MoveUpAsync(elementName, index);
        return RedirectToAction("Index", new { elementName });

    }

    public void PopulateViewData(string elementName)
    {
        ElementName = elementName;
        MenuId = "Indexes";
    }

    private async Task PopulateCheckBoxAsync(string elementName, string index)
    {
        var formElement = await indexesService.GetFormElementAsync(elementName);
        var listItems = new List<SelectListItem>();

        foreach (var field in formElement.Fields)
        {
            string name = field.Name;
            if (!string.IsNullOrEmpty(field.Label))
                name += $" ({field.Label})";

            if (!string.IsNullOrEmpty(index))
            {
                bool isContain = formElement.Indexes[int.Parse(index)].Columns.Contains(field.Name);
                listItems.Add(isContain
                    ? new SelectListItem { Text = name, Value = field.Name, Selected = true }
                    : new SelectListItem { Text = name, Value = field.Name, Selected = false });
            }
            else
            {
                listItems.Add(new SelectListItem { Text = name, Value = field.Name, Selected = false });
            }

        }

        CheckboxList = listItems;
    }
}