
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Abstractions;
using JJMasterData.Web.Areas.DataDictionary.Models;
using Microsoft.AspNetCore.Mvc;


namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class EntityController(
    EntityService entityService,
    IFormEventHandlerResolver? formEventHandlerFactory = null,
    IGridEventHandlerResolver? gridEventHandlerResolver = null)
    : DataDictionaryController
{
    public async Task<IActionResult> Index(string elementName)
    {
        return View(await Populate(elementName, true));
    }
    
    [HttpPost]        
    public async Task<ActionResult> Index(
        EntityViewModel model)
    {
        var entity = await entityService.EditEntityAsync(model.Entity, model.ElementName);

        if (entity != null)
        {
            return View(model);
        }
            
        return View(model);

    }
    
    public async Task<IActionResult> Edit(string elementName)
    {
        return View(await Populate(elementName, false));
    }

    [HttpPost]        
    public async Task<ActionResult> Edit(
        EntityViewModel model)
    {
        var entity = await entityService.EditEntityAsync(model.Entity, model.ElementName);

        if (entity != null)
        {
            return RedirectToAction("Index", new { elementName = entity.Name });
        }


        return View(model);

    }

    private async Task<EntityViewModel> Populate(string elementName, bool readOnly)
    {
        var formElement = await entityService.GetFormElementAsync(elementName);
        var entity = Entity.FromFormElement(formElement);
        var viewModel = new EntityViewModel
        {
            Entity = entity,
            ElementName = elementName,
            FormEvent = formEventHandlerFactory?.GetFormEventHandler(elementName) as IEventHandler ?? gridEventHandlerResolver?.GetGridEventHandler(elementName),
            Disabled = readOnly
        };
    
        return viewModel;
    }


}