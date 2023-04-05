using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

[Area("DataDictionary")]
public class RelationshipsController : DataDictionaryController
{
    private readonly RelationshipsService _relationshipsService;

    public RelationshipsController(RelationshipsService relationshipsService)
    {
        _relationshipsService = relationshipsService;
    }

    public ActionResult Index(string dictionaryName)
    {
        List<ElementRelationship> listRelation = _relationshipsService.GetFormElement(dictionaryName).Relationships;
        PopulateViewBag(dictionaryName);

        return View(listRelation);
    }

    public ActionResult Detail(string dictionaryName, string index)
    {
        ElementRelationship elementRelationship;
        if (!string.IsNullOrEmpty(index))
            elementRelationship = _relationshipsService.GetFormElement(dictionaryName).Relationships[int.Parse(index)];
        else
            elementRelationship = new ElementRelationship();

        PopulatePkTable();
        PopulateViewBag(dictionaryName);
        PopulatePkColumn(dictionaryName);
        PopulateFkColumn(elementRelationship.ChildElement);
        ViewBag.Index = index;

        return View(elementRelationship);
    }

    [HttpPost]
    public ActionResult CreateRelation(string dictionaryName, string? index, ElementRelationship elementRelationship, string pk, string fk)
    {

        if (_relationshipsService.ValidateFinallyAddRelation(elementRelationship, pk, fk))
        {
            elementRelationship.Columns.Add(new ElementRelationColumn(pk, fk));
        }
        else
        {
            var summary = _relationshipsService.GetValidationSummary();
            ViewBag.ErrorItem = summary.GetHtml();
        }

        PopulatePkTable();
        PopulateViewBag(dictionaryName);
        PopulatePkColumn(dictionaryName);
        PopulateFkColumn(elementRelationship.ChildElement);
        if (index != null) 
            ViewBag.Index = index;

        return View("Detail", elementRelationship);
    }

    [HttpPost]
    public ActionResult SaveRelation(string dictionaryName, ElementRelationship elementRelationship, string? index)
    {

        if (_relationshipsService.ValidateFields(elementRelationship, dictionaryName, index))
        {
            _relationshipsService.Save(elementRelationship, index, dictionaryName);
            return Json(new { success = true });
        }

        var jjSummary = _relationshipsService.GetValidationSummary();
        return Json(new { success = false, errorMessage = jjSummary.GetHtml() });

    }

    [HttpPost]
    public ActionResult DeleteRelation(string dictionaryName, string index, string indexRelation, ElementRelationship elementRelationship)
    {
        int indexRelationConvert = int.Parse(indexRelation);
        elementRelationship.Columns.Remove(elementRelationship.Columns[indexRelationConvert]);

        PopulatePkTable();
        PopulateViewBag(dictionaryName);
        PopulatePkColumn(dictionaryName);
        PopulateFkColumn(elementRelationship.ChildElement);
        ViewBag.Index = index;

        return View("Detail", elementRelationship);
    }

    [HttpPost]
    public ActionResult Index(string dictionaryName, string filter)
    {

        List<ElementRelationship> listRelation = _relationshipsService.GetFormElement(dictionaryName).Relationships;
        listRelation = listRelation.Where(l => l.Columns.Any(x => x.FkColumn.Contains(filter.ToUpper()) || x.PkColumn.Contains(filter.ToUpper()))).ToList();

        PopulateViewBag(dictionaryName);

        return View(listRelation);
    }

    [HttpPost]
    public ActionResult Detail(string dictionaryName, ElementRelationship elementRelationship, string? index)
    {
        PopulatePkTable();
        PopulateViewBag(dictionaryName);
        PopulatePkColumn(dictionaryName);
        elementRelationship.Title = PopulateFkColumn(elementRelationship.ChildElement);
        if (index != null) 
            ViewBag.Index = index;
        return View(elementRelationship);
    }

    [HttpPost]
    public ActionResult Delete(string dictionaryName, string index)
    {
        _relationshipsService.Delete(dictionaryName, index);
        return RedirectToAction("Index", new { dictionaryName });
    }

    [HttpPost]
    public ActionResult MoveDown(string dictionaryName, string index)
    {
        _relationshipsService.MoveDown(dictionaryName, index);
        return RedirectToAction("Index", new { dictionaryName });
    }

    [HttpPost]
    public ActionResult MoveUp(string dictionaryName, string index)
    {
        _relationshipsService.MoveUp(dictionaryName, index);
        return RedirectToAction("Index", new { dictionaryName });
    }

    private void PopulateViewBag(string dictionaryName)
    {
        ViewBag.DictionaryName = dictionaryName;
        ViewBag.MenuId = "Relations";
    }

    public void PopulatePkColumn(string dictionaryName)
    {
        FormElement formElement = _relationshipsService.GetFormElement(dictionaryName);
        List<SelectListItem> listItem = new List<SelectListItem>();

        foreach (var field in formElement.Fields)
        {
            listItem.Add(new SelectListItem(field.Name, field.Name));
        }

        ViewBag.PkColumn = listItem;
    }

    private string PopulateFkColumn(string childElement)
    {
        List<SelectListItem> listItem = new List<SelectListItem>();

        string title = "";
        if (string.IsNullOrEmpty(childElement))
        {
            listItem.Add(new SelectListItem(Translate.Key("(Select)"), ""));
        }
        else
        {
            Metadata dicParser = _relationshipsService.DataDictionaryRepository.GetMetadata(childElement);
            title = dicParser.Form.Title;
            foreach (var field in dicParser.Table.Fields)
            {
                listItem.Add(new SelectListItem(field.Name, field.Name));
            }

        }

        ViewBag.FkColumn = listItem;
        return title;

    }

    public void PopulatePkTable()
    {
        var listItem = new List<SelectListItem>();
        IEnumerable<string> list = _relationshipsService.DataDictionaryRepository.GetNameList();

        foreach (string name in list)
        {
            listItem.Add(new SelectListItem(name, name));
        }

        ViewBag.PkTable = listItem;
    }

}