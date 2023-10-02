using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class IndexesController : DataDictionaryController
{
    private readonly IndexesService _indexesService;

    public IndexesController(IndexesService indexesService)
    {
        _indexesService = indexesService;
    }

    public async Task<ActionResult> Index(string elementName)
    {
        List<ElementIndex> listIndexes = (await _indexesService.GetFormElementAsync(elementName)).Indexes;
        PopulateViewBag(elementName);

        return View(listIndexes);
    }

    public async Task<ActionResult> Detail(string elementName, string index)
    {
        FormElement formElement = await _indexesService.GetFormElementAsync(elementName);

        ElementIndex elementIndex;
        if (!string.IsNullOrEmpty(index))
        {
            elementIndex = formElement.Indexes[int.Parse(index)];
            ViewBag.IndexName = $"IX_{formElement.Name}_{int.Parse(index) + 1}";
        }
        else
        {
            elementIndex = new ElementIndex();
            ViewBag.IndexName = $"IX_{formElement.Name}_{formElement.Indexes.Count}";
        }

        await PopulateCheckBoxAsync(elementName, index);
        PopulateViewBag(elementName);
        return View("Detail",  elementIndex);

    }

    [HttpPost]
    public async Task<ActionResult> Detail(string elementName, string? index, List<SelectListItem> checkBoxList, ElementIndex elementIndex)
    {
        List<string> indexColumns = (from item in checkBoxList where item.Selected select item.Value).ToList();

        elementIndex.Columns = indexColumns;

        if (await _indexesService.SaveAsync(elementName, index, elementIndex))
        {
            return Json(new { success = true });
        }

        var summary = _indexesService.GetValidationSummary();
        return Json(new { success = false, errorMessage = summary.GetHtml() });
    }

    [HttpPost]
    public async Task<ActionResult> Index(string elementName, string filter)
    {
        List<ElementIndex> indexes = (await _indexesService.GetFormElementAsync(elementName)).Indexes;

        if (!string.IsNullOrEmpty(filter))
            indexes = indexes.FindAll(l => l.Columns.Contains(filter));


        PopulateViewBag(elementName);
        return View(indexes);
    }

    [HttpPost]
    public ActionResult Delete(string elementName, string index)
    {
        _indexesService.DeleteAsync(elementName, index);
        return RedirectToAction("Index", new { elementName });
    }

    [HttpPost]
    public ActionResult MoveDown(string elementName, string index)
    {
        _indexesService.MoveDownAsync(elementName, index);
        return RedirectToAction("Index", new { elementName });

    }

    [HttpPost]
    public ActionResult MoveUp(string elementName, string index)
    {
        _indexesService.MoveUpAsync(elementName, index);
        return RedirectToAction("Index", new { elementName });

    }

    public void PopulateViewBag(string elementName)
    {
        ViewBag.ElementName = elementName;
        ViewBag.MenuId = "Indexes";
    }

    private async Task PopulateCheckBoxAsync(string elementName, string index)
    {
        var formElement = await _indexesService.GetFormElementAsync(elementName);
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

        ViewBag.CheckBoxList = listItems;
    }
}