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

    #region Index

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

    [HttpPost]
    public ActionResult Delete(string dictionaryName, int selectedIndex)
    {
        _relationshipsService.Delete(dictionaryName, selectedIndex);

        return RedirectToAction("Index", new { dictionaryName });
    }

    [HttpPost]
    public ActionResult Sort(string dictionaryName, string[] relationships)
    {
        _relationshipsService.Sort(dictionaryName, relationships);
        return Ok();
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

    private RelationshipsListViewModel CreateListViewModel(string dictionaryName,
        FormElementRelationshipList relationships)
    {
        return new RelationshipsListViewModel(dictionaryName, "Relationships")
        {
            Relationships = relationships
        };
    }

    #endregion

    #region ElementDetails

    public ActionResult ElementDetails(string dictionaryName, int? selectedIndex)
    {
        var formElement = _relationshipsService.GetFormElement(dictionaryName);
        var relationships = formElement.Relationships.GetElementRelationships();
        var relationship = selectedIndex != null ? relationships[selectedIndex.Value] : new ElementRelationship();

        var model = CreateElementDetailsViewModel(dictionaryName, relationship, selectedIndex);

        return View("DetailElement", model);
    }

    [HttpPost]
    public ActionResult ElementDetails(RelationshipsElementDetailsViewModel model)
    {
        var newModel = CreateElementDetailsViewModel(model.DictionaryName, model.Relationship, model.SelectedIndex);
        return View("DetailElement", newModel);
    }

    [HttpPost]
    public ActionResult CreateRelationship(RelationshipsElementDetailsViewModel model)
    {
        if (_relationshipsService.ValidateFinallyAddRelation(model.DictionaryName, model.Relationship,
                model.AddPrimaryKeyName!, model.AddForeignKeyName!))
        {
            model.Relationship.Columns.Add(new ElementRelationshipColumn(model.AddPrimaryKeyName!,
                model.AddForeignKeyName!));
        }
        else
        {
            model.ValidationSummary = _relationshipsService.GetValidationSummary();
        }

        PopulateSelectLists(model);

        return View("DetailElement", model);
    }

    [HttpPost]
    public ActionResult SaveRelationshipElement(RelationshipsElementDetailsViewModel model)
    {
        if (_relationshipsService.ValidateElementRelationship(model.Relationship, model.DictionaryName, model.SelectedIndex))
        {
            _relationshipsService.SaveElementRelationship(model.Relationship, model.SelectedIndex, model.DictionaryName);
            return Json(new { success = true });
        }

        PopulateSelectLists(model);

        var jjSummary = _relationshipsService.GetValidationSummary();
        return Json(new { success = false, errorMessage = jjSummary.GetHtml() });
    }

    [HttpPost]
    public ActionResult DeleteRelationshipColumn(RelationshipsElementDetailsViewModel model, int columnIndex)
    {
        model.Relationship.Columns.RemoveAt(columnIndex);

        PopulateSelectLists(model);

        return View("DetailElement", model);
    }

    private void PopulateSelectLists(RelationshipsElementDetailsViewModel model)
    {
        model.ElementsSelectList = GetElementsSelectList(model.Relationship.ChildElement);
        model.ForeignKeysSelectList = GetForeignKeysSelectList(model.Relationship.ChildElement);
        model.PrimaryKeysSelectList = GetPrimaryKeysSelectList(model.DictionaryName);
    }

    private RelationshipsElementDetailsViewModel CreateElementDetailsViewModel(
        string dictionaryName,
        ElementRelationship relationship, int? selectedIndex = null)
    {
        return new RelationshipsElementDetailsViewModel(dictionaryName, "Relationships")
        {
            Relationship = relationship,
            ElementsSelectList = GetElementsSelectList(relationship.ChildElement),
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
            selectList.Add(new SelectListItem(Translate.Key("(Select)"), string.Empty));
        }
        else
        {
            var dicParser = _relationshipsService.DataDictionaryRepository.GetMetadata(childDictionaryName);
            selectList.AddRange(dicParser.Table.Fields.Select(field => new SelectListItem(field.Name, field.Name)));
        }

        return selectList;
    }

    private List<SelectListItem> GetElementsSelectList(string childDictionaryName)
    {
        IEnumerable<string> list = _relationshipsService.DataDictionaryRepository.GetNameList();

        var selectList = list.Select(name => new SelectListItem(name, name)).ToList();

        if (string.IsNullOrEmpty(childDictionaryName))
        {
            selectList.Insert(0, new SelectListItem(Translate.Key("(Select)"), string.Empty));
        }

        return selectList;
    }

    #endregion

    #region LayoutDetails

    [HttpGet]
    public IActionResult LayoutDetails(string dictionaryName, int index)
    {
        var model = CreateLayoutDetailsViewModel(dictionaryName, index);
        return View("DetailLayout", model);
    }
    
    [HttpPost]
    public IActionResult LayoutDetails(RelationshipsLayoutDetailsViewModel model)
    {

        return View("DetailLayout", model);
    }
    
    public IActionResult SaveRelationshipLayout(RelationshipsLayoutDetailsViewModel model, FormElementPanel panel)
    {

        var formElement = _relationshipsService.DataDictionaryRepository.GetMetadata(model.DictionaryName).GetFormElement();

        var relationship = formElement.Relationships[model.Index];

        relationship.ViewType = model.ViewType;
        relationship.Panel = panel;

        if (_relationshipsService.ValidateFormElementRelationship(relationship))
        {
            _relationshipsService.SaveFormElementRelationship(relationship, model.Index, model.DictionaryName);
            return Json(new { success = true });
        }

        var jjSummary = _relationshipsService.GetValidationSummary();
        return Json(new { success = false, errorMessage = jjSummary.GetHtml() });
    }

    private RelationshipsLayoutDetailsViewModel CreateLayoutDetailsViewModel(
        string dictionaryName,
        int index)
    {
        var formElement = (FormElement)_relationshipsService.DataDictionaryRepository.GetMetadata(dictionaryName);

        var relationship = formElement.Relationships[index];
        
        return new RelationshipsLayoutDetailsViewModel(dictionaryName, "Relationships")
        {
            Index = index,
            IsParent = relationship.IsParent,
            Panel = relationship.Panel,
            ViewType = relationship.ViewType
        };
    }
    
    #endregion


}