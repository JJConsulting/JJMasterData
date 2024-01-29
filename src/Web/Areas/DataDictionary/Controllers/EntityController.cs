using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Abstractions;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class EntityController(EntityService entityService, IFormEventHandlerResolver? formEventHandlerFactory = null,
        IGridEventHandlerResolver? gridEventHandlerResolver = null)
    : DataDictionaryController
{
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
        var entity = await entityService.EditEntityAsync(model.FormElement, model.ElementName);

        if (entity != null)
        {
            return RedirectToAction("Index", new { elementName = entity.Name });
        }

        model.MenuId = "Entity";
        model.ValidationSummary = entityService.GetValidationSummary();
            
        return View(model);

    }

    private async Task<EntityViewModel> Populate(string elementName, bool readOnly)
    {
        var viewModel = new EntityViewModel(menuId:"Entity", elementName:elementName)
        {
            FormElement = await entityService.GetFormElementAsync(elementName),
            FormEvent = formEventHandlerFactory?.GetFormEventHandler(elementName) as IEventHandler ?? gridEventHandlerResolver?.GetGridEventHandler(elementName),
            Disabled = readOnly
        };

        return viewModel;
    }


}