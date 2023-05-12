using JJMasterData.Commons.Data;
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

    [HttpDelete]
    public ActionResult Delete(string dictionaryName, int id)
    {
        _relationshipsService.Delete(dictionaryName, id);
        return RedirectToAction("Index", new { dictionaryName });
    }

    [HttpPost]
    public ActionResult Sort(string dictionaryName, [FromBody] string[] relationships)
    {
        _relationshipsService.Sort(dictionaryName, relationships);
        return Ok();
    }

    
    private static RelationshipsListViewModel CreateListViewModel(string dictionaryName,
        FormElementRelationshipList relationships)
    {
        return new RelationshipsListViewModel(dictionaryName, "Relationships")
        {
            Relationships = relationships
        };
    }

    #endregion

    #region ElementDetails

    public ActionResult ElementDetails(string dictionaryName, int? id)
    {
        var formElement = _relationshipsService.GetFormElement(dictionaryName);
        var relationship = id != null ? formElement.Relationships.GetById(id.Value).ElementRelationship! : new ElementRelationship();

        var model = CreateElementDetailsViewModel(dictionaryName, relationship, id);

        return View("DetailElement", model);
    }

    [HttpPost]
    public ActionResult ElementDetails(RelationshipsElementDetailsViewModel model)
    {
        var newModel = CreateElementDetailsViewModel(model.DictionaryName, model.Relationship, model.Id);
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
        if (_relationshipsService.ValidateElementRelationship(model.Relationship, model.DictionaryName, model.Id))
        {
            _relationshipsService.SaveElementRelationship(model.Relationship, model.Id, model.DictionaryName);
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
        ElementRelationship relationship, int? id = null)
    {
        return new RelationshipsElementDetailsViewModel(dictionaryName, "Relationships")
        {
            Id = id,
            Relationship = relationship,
            ElementsSelectList = GetElementsSelectList(relationship.ChildElement),
            PrimaryKeysSelectList = GetPrimaryKeysSelectList(dictionaryName),
            ForeignKeysSelectList = GetForeignKeysSelectList(relationship.ChildElement)
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
            var formElement = _relationshipsService.DataDictionaryRepository.GetMetadata(childDictionaryName);
            selectList.AddRange(formElement.Fields.Select(field => new SelectListItem(field.Name, field.Name)));
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
    public IActionResult LayoutDetails(string dictionaryName, int id)
    {
        var model = CreateLayoutDetailsViewModel(dictionaryName, id);
        return View("DetailLayout", model);
    }
    
    [HttpPost]
    public IActionResult LayoutDetails(RelationshipsLayoutDetailsViewModel model)
    {

        return View("DetailLayout", model);
    }
    
    public IActionResult SaveRelationshipLayout(RelationshipsLayoutDetailsViewModel model, FormElementPanel panel)
    {
        if (_relationshipsService.ValidatePanel(panel))
        {
            _relationshipsService.SaveFormElementRelationship(panel,model.ViewType, model.Id, model.DictionaryName);
            return Json(new { success = true });
        }

        var jjSummary = _relationshipsService.GetValidationSummary();
        return Json(new { success = false, errorMessage = jjSummary.GetHtml() });
    }

    private RelationshipsLayoutDetailsViewModel CreateLayoutDetailsViewModel(
        string dictionaryName,
        int id)
    {
        var formElement = _relationshipsService.DataDictionaryRepository.GetMetadata(dictionaryName);

        var relationship = formElement.Relationships.GetById(id);
        
        return new RelationshipsLayoutDetailsViewModel(dictionaryName, "Relationships")
        {
            Id = id,
            IsParent = relationship.IsParent,
            Panel = relationship.Panel,
            ViewType = relationship.ViewType
        };
    }
    
    #endregion
}