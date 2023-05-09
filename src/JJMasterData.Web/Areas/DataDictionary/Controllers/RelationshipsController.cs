using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class RelationshipsController : DataDictionaryController
{
    private readonly RelationshipsService _relationshipsService;

    public RelationshipsController(RelationshipsService relationshipsService)
    {
        _relationshipsService = relationshipsService;
    }

    public ActionResult Index(string dictionaryName)
    {
        var relationships = _relationshipsService.GetFormElement(dictionaryName).Relationships;

        var model = CreateListViewModel(dictionaryName, relationships);
        
        return View(model);
    }

    public ActionResult Detail(string dictionaryName, int? selectedIndex)
    {
        var formElement = _relationshipsService.GetFormElement(dictionaryName);
        var relationships = formElement.Relationships.GetElementRelationships();
        var relationship = selectedIndex != null ? relationships[selectedIndex.Value] : new ElementRelationship();

        var model = CreateDetailsViewModel(dictionaryName, relationship, selectedIndex);

        return View(model);
    }
    
    [HttpPost]
    public ActionResult Detail(RelationshipsDetailsViewModel model)
    {
        var newModel = CreateDetailsViewModel(model.DictionaryName, model.Relationship, model.SelectedIndex);
        return View(newModel);
    }

    [HttpPost]
    public ActionResult CreateRelation(string dictionaryName, ElementRelationship elementRelationship, string pk, string fk)
    {

        var model = CreateDetailsViewModel(dictionaryName, elementRelationship);
        
        if (_relationshipsService.ValidateFinallyAddRelation(dictionaryName, elementRelationship, pk, fk))
        {
            elementRelationship.Columns.Add(new ElementRelationColumn(pk, fk));
        }
        else
        {
            model.ValidationSummary =  _relationshipsService.GetValidationSummary();
        }
        
        return View("Detail", model);
    }

    [HttpPost]
    public ActionResult SaveRelation(RelationshipsDetailsViewModel model)
    {

        if (_relationshipsService.ValidateFields(model.Relationship, model.DictionaryName, model.SelectedIndex))
        {
            _relationshipsService.Save(model.Relationship, model.SelectedIndex, model.DictionaryName);
            return Json(new { success = true });
        }

        var jjSummary = _relationshipsService.GetValidationSummary();
        return Json(new { success = false, errorMessage = jjSummary.GetHtml() });

    }

    [HttpPost]
    public ActionResult DeleteRelation(RelationshipsDetailsViewModel model, int selectedIndex)
    {
        model.Relationship.Columns.RemoveAt(selectedIndex);

        return View("Detail", model);
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

    private RelationshipsListViewModel CreateListViewModel(string dictionaryName, FormElementRelationshipList relationships)
    {
        return new RelationshipsListViewModel(dictionaryName, "Relationships")
        {
            Relationships = relationships
        };
    }
    
    private RelationshipsDetailsViewModel CreateDetailsViewModel(
        string dictionaryName,
        ElementRelationship relationship, int? selectedIndex = null)
    {
        return new RelationshipsDetailsViewModel(dictionaryName, "Relationships")
        {
            Relationship = relationship,
            ElementsSelectList = GetElementsSelectList(),
            PrimaryKeysSelectList = GetPrimaryKeysSelectList(dictionaryName),
            ForeignKeysSelectList = GetForeignKeysSelectList(relationship.ChildElement),
            SelectedIndex = selectedIndex
        };
    }

    public List<SelectListItem> GetPrimaryKeysSelectList(string dictionaryName)
    {
        var formElement = _relationshipsService.GetFormElement(dictionaryName);
        var selectList = formElement.Fields.Select(field => new SelectListItem(field.Name, field.Name)).ToList();

        return selectList;
    }

    private List<SelectListItem> GetForeignKeysSelectList(string childDictionaryName)
    {
        var selectList = new List<SelectListItem>();
        
        if (string.IsNullOrEmpty(childDictionaryName))
        {
            selectList.Add(new SelectListItem(Translate.Key("(Select)"), ""));
        }
        else
        {
            var dicParser = _relationshipsService.DataDictionaryRepository.GetMetadata(childDictionaryName);
            selectList.AddRange(dicParser.Table.Fields.Select(field => new SelectListItem(field.Name, field.Name)));
        }

        return selectList;
    }

    private List<SelectListItem> GetElementsSelectList()
    {
        IEnumerable<string> list = _relationshipsService.DataDictionaryRepository.GetNameList();

        var selectList = list.Select(name => new SelectListItem(name, name)).ToList();

       return selectList;
    }

}