using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.FormEvents;
using JJMasterData.Web.Areas.DataDictionary.Models;
using JJMasterData.Web.Controllers;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

[Area("DataDictionary")]
public class EntityController : DataDictionaryController
{
    private readonly EntityService _entityService;

    public EntityController(EntityService entityService)
    {
        _entityService = entityService;
    }

    public ActionResult Index(string dictionaryName)
    {
        return View(Populate(dictionaryName, true));
    }

    public ActionResult Edit(string dictionaryName)
    {
        return View(Populate(dictionaryName, false));
    }

    [HttpPost]        
    public ActionResult Edit(
        EntityViewModel model)
    {
        var entity = _entityService.EditEntity(model.FormElement, model.DictionaryName);

        if (entity != null)
        {
            return RedirectToAction("Index", new { dictionaryName = entity.Name });
        }

        model.MenuId = "Entity";
        model.ValidationSummary = _entityService.GetValidationSummary();
            
        return View(model);

    }

    private EntityViewModel Populate(string dictionaryName, bool readOnly)
    {
        var viewModel = new EntityViewModel
        {
            MenuId = "Entity",
            DictionaryName = dictionaryName,
            FormElement = _entityService.GetFormElement(dictionaryName),
            FormEvent = FormEventManager.GetFormEvent(dictionaryName),
            ReadOnly = readOnly
        };

        return viewModel;
    }


}