using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Abstractions;
using JJMasterData.Web.Areas.DataDictionary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class EntityController(
    EntityService entityService,
    IOptions<MasterDataCommonsOptions> options,
    IFormEventHandlerResolver? formEventHandlerFactory = null,
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
        var entity = await entityService.EditEntityAsync(model.Entity, model.ElementName);

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
        var formElement = await entityService.GetFormElementAsync(elementName);
        var entity = Entity.FromFormElement(formElement);
        var viewModel = new EntityViewModel(menuId:"Entity", elementName:elementName)
        {
            Entity = entity,
            FormEvent = formEventHandlerFactory?.GetFormEventHandler(elementName) as IEventHandler ?? gridEventHandlerResolver?.GetGridEventHandler(elementName),
            Disabled = readOnly
        };

        if (string.IsNullOrEmpty(viewModel.Entity.ReadProcedureName))
            entity.ReadProcedureName = options.Value.GetReadProcedureName(formElement);

        if (string.IsNullOrEmpty(viewModel.Entity.WriteProcedureName))
            entity.WriteProcedureName = options.Value.GetWriteProcedureName(formElement);
    
        return viewModel;
    }


}