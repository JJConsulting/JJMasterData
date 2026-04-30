using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Filters;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class ValidationsController(FormElementValidationsService formElementValidationsService) : DataDictionaryController
{
    [ImportModelState]
    public async Task<IActionResult> Index(string elementName, int? validationId = null)
    {
        var formElement = await formElementValidationsService.GetFormElementAsync(elementName);
        FormElementValidation validation;

        if (validationId == null)
        {
            if (TempData.ContainsKey("validation"))
                validation = TempData.Get<FormElementValidation>("validation")!;
            else if (formElement.Validations.Count > 0)
                validation = formElement.Validations[0];
            else
                validation = new FormElementValidation();
        }
        else
        {
            validation = formElement.GetValidationById(validationId.Value);
        }

        PopulateViewData(formElement, validation);
        return View(validation);
    }

    public async Task<IActionResult> Detail(string elementName, int validationId)
    {
        var formElement = await formElementValidationsService.GetFormElementAsync(elementName);
        var validation = formElement.GetValidationById(validationId);
        PopulateViewData(formElement, validation);
        return PartialView("_Detail", validation);
    }

    public async Task<IActionResult> Add(string elementName)
    {
        var formElement = await formElementValidationsService.GetFormElementAsync(elementName);
        var validation = new FormElementValidation();
        PopulateViewData(formElement, validation);
        return PartialView("_Detail", validation);
    }

    [HttpPost]
    [ExportModelState]
    public async Task<IActionResult> Save(string elementName, FormElementValidation validation)
    {
        await formElementValidationsService.SaveAsync(elementName, validation);
        if (ModelState.IsValid)
            return RedirectToAction("Index", new { elementName, validationId = validation.Id });

        return RedirectToIndex(elementName, validation);
    }

    public async Task<IActionResult> Delete(string elementName, int validationId)
    {
        await formElementValidationsService.DeleteAsync(elementName, validationId);
        return RedirectToAction("Index", new { elementName });
    }

    [HttpPost]
    public async Task<IActionResult> Index(string elementName, FormElementValidation validation)
    {
        var formElement = await formElementValidationsService.GetFormElementAsync(elementName);
        PopulateViewData(formElement, validation);
        return View(validation);
    }

    private RedirectToActionResult RedirectToIndex(string elementName, FormElementValidation validation)
    {
        TempData.Put("validation", validation);

        return RedirectToAction("Index", new { elementName });
    }

    private void PopulateViewData(FormElement formElement, FormElementValidation validation)
    {
        ViewData["MenuId"] = "Validations";
        ViewData["ElementName"] = formElement.Name;
        ViewData["CodeEditorHints"] = formElement.Fields.Select(f => new CodeEditorHint
        {
            Language = "sql",
            InsertText = f.Name,
            Label = f.Name,
            Details = "Form Element Field",
        }).ToList();
        ViewBag.ValidationId = validation.Id;
        ViewBag.Validations = formElement.Validations;
    }
}
