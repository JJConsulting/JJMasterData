using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
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

    public async Task<ActionResult> Index(string dictionaryName)
    {
        List<ElementIndex> listIndexes = (await _indexesService.GetFormElementAsync(dictionaryName)).Indexes;
        PopulateViewBag(dictionaryName);

        return View(listIndexes);
    }

    public async Task<ActionResult> Detail(string dictionaryName, string index)
    {
        FormElement formElement = await _indexesService.GetFormElementAsync(dictionaryName);

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

        PopulateCheckBox(dictionaryName, index);
        PopulateViewBag(dictionaryName);
        return View("Detail",  elementIndex);

    }

    [HttpPost]
    public ActionResult Detail(string dictionaryName, string? index, List<SelectListItem> checkBoxList, ElementIndex elementIndex)
    {
        List<string> indexColumns = (from item in checkBoxList where item.Selected select item.Value).ToList();

        elementIndex.Columns = indexColumns;

        if (_indexesService.Save(dictionaryName, index, elementIndex))
        {
            return Json(new { success = true });
        }

        var summary = _indexesService.GetValidationSummary();
        return Json(new { success = false, errorMessage = summary.GetHtml() });
    }

    [HttpPost]
    public async Task<ActionResult> Index(string dictionaryName, string filter)
    {
        List<ElementIndex> indexes = (await _indexesService.GetFormElementAsync(dictionaryName)).Indexes;

        if (!string.IsNullOrEmpty(filter))
            indexes = indexes.FindAll(l => l.Columns.Contains(filter));


        PopulateViewBag(dictionaryName);
        return View(indexes);
    }

    [HttpPost]
    public ActionResult Delete(string dictionaryName, string index)
    {
        _indexesService.Delete(dictionaryName, index);
        return RedirectToAction("Index", new { dictionaryName });
    }

    [HttpPost]
    public ActionResult MoveDown(string dictionaryName, string index)
    {
        _indexesService.MoveDown(dictionaryName, index);
        return RedirectToAction("Index", new { dictionaryName });

    }

    [HttpPost]
    public ActionResult MoveUp(string dictionaryName, string index)
    {
        _indexesService.MoveUp(dictionaryName, index);
        return RedirectToAction("Index", new { dictionaryName });

    }

    public void PopulateViewBag(string dictionaryName)
    {
        ViewBag.DictionaryName = dictionaryName;
        ViewBag.MenuId = "Indexes";
    }

    private async Task PopulateCheckBox(string dictionaryName, string index)
    {
        var formElement = await _indexesService.GetFormElementAsync(dictionaryName);
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