using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Abstractions;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class EntityController : DataDictionaryController
{
    private readonly EntityService _entityService;
    private readonly IFormEventHandlerResolver? _formEventHandlerFactory;
    private readonly IGridEventHandlerResolver? _gridEventHandlerResolver;

    public EntityController(EntityService entityService, IFormEventHandlerResolver? formEventHandlerFactory = null, IGridEventHandlerResolver? gridEventHandlerResolver = null)
    {
        _entityService = entityService;
        _formEventHandlerFactory = formEventHandlerFactory;
        _gridEventHandlerResolver = gridEventHandlerResolver;
    }

    public async Task<IActionResult> Index(string elementName)
    {
        return View(await Populate(elementName, true));
    }

    public async Task<IActionResult> Edit(string elementName)
    {
        return View(await Populate(elementName, false));
    }

    [HttpPost]        
    public async Task<ActionResult> Edit(
        EntityViewModel model)
    {
        var entity = await _entityService.EditEntityAsync(model.FormElement, model.ElementName);

        if (entity != null)
        {
            return RedirectToAction("Index", new { elementName = entity.Name });
        }

        model.MenuId = "Entity";
        model.ValidationSummary = _entityService.GetValidationSummary();
            
        return View(model);

    }

    private async Task<EntityViewModel> Populate(string elementName, bool readOnly)
    {
        var viewModel = new EntityViewModel(menuId:"Entity", elementName:elementName)
        {
            FormElement = await _entityService.GetFormElementAsync(elementName),
            FormEvent = _formEventHandlerFactory?.GetFormEventHandler(elementName) as IEventHandler ?? _gridEventHandlerResolver?.GetGridEventHandler(elementName),
            ReadOnly = readOnly
        };

        return viewModel;
    }


}