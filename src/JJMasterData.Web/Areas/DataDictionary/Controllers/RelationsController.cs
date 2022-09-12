using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

[Area("DataDictionary")]
public class RelationsController : DataDictionaryController
{
    private readonly RelationsService _relationsService;

    public RelationsController(RelationsService relationsService) 
    {
        _relationsService = relationsService;
    }

    public ActionResult Index(string dictionaryName)
    {
        List<ElementRelation> listRelation = _relationsService.GetFormElement(dictionaryName).Relations;
        PopulateViewBag(dictionaryName);

        return View(listRelation);
    }

    public ActionResult Detail(string dictionaryName, string index)
    {
        ElementRelation elementRelation;
        if (!string.IsNullOrEmpty(index))
            elementRelation = _relationsService.GetFormElement(dictionaryName).Relations[int.Parse(index)];
        else
            elementRelation = new ElementRelation();

        PopulatePkTable();
        PopulateViewBag(dictionaryName);
        PopulatePkColumn(dictionaryName);
        PopulateFkColumn(elementRelation.ChildElement);
        ViewBag.Index = index;

        return View(elementRelation);
    }

    [HttpPost]
    public ActionResult CreateRelation(string dictionaryName, string? index, ElementRelation elementRelation, string pk, string fk)
    {

        if (_relationsService.ValidateFinallyAddRelation(elementRelation, pk, fk))
        {
            elementRelation.Columns.Add(new ElementRelationColumn(pk, fk));
        }
        else
        {
            var summary = _relationsService.GetValidationSummary();
            ViewBag.ErrorItem = summary.GetHtml();
        }

        PopulatePkTable();
        PopulateViewBag(dictionaryName);
        PopulatePkColumn(dictionaryName);
        PopulateFkColumn(elementRelation.ChildElement);
        ViewBag.Index = index;

        return View("Detail", elementRelation);
    }

    [HttpPost]
    public ActionResult SaveRelation(string dictionaryName, ElementRelation elementRelation, string? index)
    {

        if (_relationsService.ValidateFields(elementRelation, dictionaryName, index))
        {
            _relationsService.Save(elementRelation, index, dictionaryName);
            return Json(new { success = true });
        }

        var jjSummary = _relationsService.GetValidationSummary();
        return Json(new { success = false, errorMessage = jjSummary.GetHtml() });

    }

    [HttpPost]
    public ActionResult DeleteRelation(string dictionaryName, string index, string indexRelation, ElementRelation elementRelation)
    {
        int indexRelationConvert = int.Parse(indexRelation);
        elementRelation.Columns.Remove(elementRelation.Columns[indexRelationConvert]);

        PopulatePkTable();
        PopulateViewBag(dictionaryName);
        PopulatePkColumn(dictionaryName);
        PopulateFkColumn(elementRelation.ChildElement);
        ViewBag.Index = index;

        return View("Detail", elementRelation);
    }

    [HttpPost]
    public ActionResult Index(string dictionaryName, string filter)
    {

        List<ElementRelation> listRelation = _relationsService.GetFormElement(dictionaryName).Relations;
        listRelation = listRelation.Where(l => l.Columns.Any(x => x.FkColumn.Contains(filter.ToUpper()) || x.PkColumn.Contains(filter.ToUpper()))).ToList();

        PopulateViewBag(dictionaryName);

        return View(listRelation);
    }

    [HttpPost]
    public ActionResult Detail(string dictionaryName, ElementRelation elementRelation, string? index)
    {
        PopulatePkTable();
        PopulateViewBag(dictionaryName);
        PopulatePkColumn(dictionaryName);
        elementRelation.Title = PopulateFkColumn(elementRelation.ChildElement);
        ViewBag.Index = index;
        return View(elementRelation);
    }

    [HttpPost]
    public ActionResult Delete(string dictionaryName, string index)
    {
        FormElement formElement = _relationsService.DicDao.GetFormElement(dictionaryName);

        ElementRelation elementRelation = formElement.Relations[int.Parse(index)];
        formElement.Relations.Remove(elementRelation);
        _relationsService.DicDao.SetFormElement(formElement);

        return RedirectToAction("Index", new { dictionaryName });
    }

    [HttpPost]
    public ActionResult MoveDown(string dictionaryName, string index)
    {
        FormElement formElement = _relationsService.DicDao.GetFormElement(dictionaryName);

        int indexToMoveDown = int.Parse(index);
        if (indexToMoveDown >= 0 && indexToMoveDown < formElement.Relations.Count - 1)
        {
            ElementRelation elementRelation = formElement.Relations[indexToMoveDown + 1];
            formElement.Relations[indexToMoveDown + 1] = formElement.Relations[indexToMoveDown];
            formElement.Relations[indexToMoveDown] = elementRelation;
            _relationsService.DicDao.SetFormElement(formElement);
        }

        return RedirectToAction("Index", new { dictionaryName });

    }

    [HttpPost]
    public ActionResult MoveUp(string dictionaryName, string index)
    {
        FormElement formElement = _relationsService.DicDao.GetFormElement(dictionaryName);

        int indexToMoveUp = int.Parse(index);
        if (indexToMoveUp > 0)
        {
            ElementRelation elementRelation = formElement.Relations[indexToMoveUp - 1];
            formElement.Relations[indexToMoveUp - 1] = formElement.Relations[indexToMoveUp];
            formElement.Relations[indexToMoveUp] = elementRelation;
            _relationsService.DicDao.SetFormElement(formElement);
        }

        return RedirectToAction("Index", new { dictionaryName });

    }

    private void PopulateViewBag(string dictionaryName)
    {
        ViewBag.DictionaryName = dictionaryName;
        ViewBag.MenuId = "Relations";
    }

    public void PopulatePkColumn(string dictionaryName)
    {
        FormElement formElement = _relationsService.GetFormElement(dictionaryName);
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
            DicParser dicParser = _relationsService.DicDao.GetDictionary(childElement);
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
        string[] list = _relationsService.DicDao.GetListDictionaryName();

        foreach (string name in list)
        {
            listItem.Add(new SelectListItem(name, name));
        }

        ViewBag.PkTable = listItem;
    }

}