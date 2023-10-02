using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class RelationshipsController : DataDictionaryController
{
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private readonly RelationshipsService _relationshipsService;

    #region Index

    public RelationshipsController(RelationshipsService relationshipsService,IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        StringLocalizer = stringLocalizer;
        _relationshipsService = relationshipsService;
    }

    public async Task<ActionResult> Index(string elementName)
    {
        var relationships = (await _relationshipsService.GetFormElementAsync(elementName)).Relationships;

        var model = CreateListViewModel(elementName, relationships);

        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Delete(string elementName, int id)
    {
        await _relationshipsService.DeleteAsync(elementName, id);
        return RedirectToAction("Index", new { elementName });
    }

    [HttpPost]
    public async Task<ActionResult> Sort(string elementName, [FromBody] string[] relationships)
    {
        await _relationshipsService.SortAsync(elementName, relationships);
        return Ok();
    }

    
    private static RelationshipsListViewModel CreateListViewModel(string elementName,
        FormElementRelationshipList relationships)
    {
        return new RelationshipsListViewModel(elementName, "Relationships")
        {
            Relationships = relationships
        };
    }

    #endregion

    #region ElementDetails

    public async Task<ActionResult> ElementDetails(string elementName, int? id)
    {
        var formElement = await _relationshipsService.GetFormElementAsync(elementName);
        var relationship = id != null ? formElement.Relationships.GetById(id.Value).ElementRelationship! : new ElementRelationship();

        var model = await CreateElementDetailsViewModel(elementName, relationship, id);

        return View("DetailElement", model);
    }

    [HttpPost]
    public async Task<ActionResult> ElementDetails(RelationshipsElementDetailsViewModel model)
    {
        var newModel = await CreateElementDetailsViewModel(model.ElementName, model.Relationship, model.Id);
        return View("DetailElement", newModel);
    }

    [HttpPost]
    public async Task<ActionResult> CreateRelationship(RelationshipsElementDetailsViewModel model)
    {
        if (await _relationshipsService.ValidateFinallyAddRelation(model.ElementName, model.Relationship,
                model.AddPrimaryKeyName!, model.AddForeignKeyName!))
        {
            model.Relationship.Columns.Add(new ElementRelationshipColumn(model.AddPrimaryKeyName!,
                model.AddForeignKeyName!));
        }
        else
        {
            model.ValidationSummary = _relationshipsService.GetValidationSummary();
        }

        await PopulateSelectLists(model);

        return View("DetailElement", model);
    }

    [HttpPost]
    public async Task<ActionResult> SaveRelationshipElement(RelationshipsElementDetailsViewModel model)
    {
        if (await _relationshipsService.ValidateElementRelationship(model.Relationship, model.ElementName, model.Id))
        {
            await _relationshipsService.SaveElementRelationship(model.Relationship, model.Id, model.ElementName);
            return Json(new { success = true });
        }

        await PopulateSelectLists(model);

        var jjSummary = _relationshipsService.GetValidationSummary();
        return Json(new { success = false, errorMessage = jjSummary.GetHtml() });
    }

    [HttpPost]
    public async Task<ActionResult> DeleteRelationshipColumn(RelationshipsElementDetailsViewModel model, int columnIndex)
    {
        model.Relationship.Columns.RemoveAt(columnIndex);

        await PopulateSelectLists(model);

        return View("DetailElement", model);
    }

    private async Task PopulateSelectLists(RelationshipsElementDetailsViewModel model)
    {
        model.ElementsSelectList = await GetElementsSelectList(model.Relationship.ChildElement);
        model.ForeignKeysSelectList = await GetForeignKeysSelectList(model.Relationship.ChildElement);
        model.PrimaryKeysSelectList = await GetPrimaryKeysSelectList(model.ElementName);
    }

    private async Task<RelationshipsElementDetailsViewModel> CreateElementDetailsViewModel(
        string elementName,
        ElementRelationship relationship, int? id = null)
    {
        return new RelationshipsElementDetailsViewModel(elementName, "Relationships")
        {
            Id = id,
            Relationship = relationship,
            ElementsSelectList = await GetElementsSelectList(relationship.ChildElement),
            PrimaryKeysSelectList = await GetPrimaryKeysSelectList(elementName),
            ForeignKeysSelectList = await GetForeignKeysSelectList(relationship.ChildElement)
        };
    }

    public async Task<List<SelectListItem>> GetPrimaryKeysSelectList(string elementName)
    {
        var formElement = await _relationshipsService.GetFormElementAsync(elementName);
        var selectList = formElement.Fields.Select(field => new SelectListItem(field.Name, field.Name)).ToList();

        return selectList;
    }

    private async Task<List<SelectListItem>> GetForeignKeysSelectList(string childElementName)
    {
        var selectList = new List<SelectListItem>();

        if (string.IsNullOrEmpty(childElementName))
        {
            selectList.Add(new SelectListItem(StringLocalizer["(Select)"], string.Empty));
        }
        else
        {
            var formElement = await _relationshipsService.DataDictionaryRepository.GetFormElementAsync(childElementName);
            selectList.AddRange(formElement.Fields.Select(field => new SelectListItem(field.Name, field.Name)));
        }

        return selectList;
    }

    private async Task<List<SelectListItem>> GetElementsSelectList(string childElementName)
    {
        IEnumerable<string> list = await _relationshipsService.DataDictionaryRepository.GetNameListAsync().ToListAsync();

        var selectList = list.Select(name => new SelectListItem(name, name)).ToList();

        if (string.IsNullOrEmpty(childElementName))
        {
            selectList.Insert(0, new SelectListItem(StringLocalizer["(Select)"], string.Empty));
        }

        return selectList;
    }

    #endregion

    #region LayoutDetails

    [HttpGet]
    public async Task<IActionResult> LayoutDetails(string elementName, int id)
    {
        var model = await CreateLayoutDetailsViewModel(elementName, id);
        return View("DetailLayout", model);
    }
    
    [HttpPost]
    public IActionResult LayoutDetails(RelationshipsLayoutDetailsViewModel model)
    {

        return View("DetailLayout", model);
    }
    
    public async Task<IActionResult> SaveRelationshipLayout(RelationshipsLayoutDetailsViewModel model, FormElementPanel panel)
    {
        if (_relationshipsService.ValidatePanel(panel))
        {
            await _relationshipsService.SaveFormElementRelationship(panel,model.ViewType, model.Id, model.ElementName);
            return Json(new { success = true });
        }

        var jjSummary = _relationshipsService.GetValidationSummary();
        return Json(new { success = false, errorMessage = jjSummary.GetHtml() });
    }

    private async Task<RelationshipsLayoutDetailsViewModel> CreateLayoutDetailsViewModel(
        string elementName,
        int id)
    {
        var formElement = await _relationshipsService.DataDictionaryRepository.GetFormElementAsync(elementName);

        var relationship = formElement.Relationships.GetById(id);
        
        return new RelationshipsLayoutDetailsViewModel(elementName, "Relationships")
        {
            Id = id,
            IsParent = relationship.IsParent,
            Panel = relationship.Panel,
            ViewType = relationship.ViewType
        };
    }
    
    #endregion
}