using System.Net;
using JJConsulting.Html.Bootstrap.Components;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Web.Areas.DataDictionary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class ElementController(
    ElementService elementService,
    ElementImportService elementImportService,
    ElementExportService elementExportService,
    ClassGenerationService classGenerationService,
    ScriptsService scriptsService,
    IEntityRepository entityRepository,
    UploadAreaFactory uploadAreaFactory,
    IStringLocalizer<MasterDataResources> stringLocalizer)
    : DataDictionaryController
{
    public async Task<IActionResult> Index()
    {
        var formView = elementService.GetFormView();
        var result = await formView.GetResultAsync();
        
        if (result is IActionResult actionResult)
            return actionResult;

        ViewData["FormViewHtml"] = result.Content;
        
        return View();
    }

    public ViewResult Add() => View(new ElementBean());

    public async Task<FileResult> Export()
    {
        var formView = elementService.GetFormView();
        var selectedRows = formView.GridView.GetSelectedGridValues();

        if (selectedRows.Count == 1)
        {
            var jsonStream = await elementExportService.ExportSingleRowAsync(selectedRows[0]);
            var jsonFileName = $"{selectedRows[0]["name"]}.json";

            Response.Headers.ContentDisposition =  $"attachment; filename=\"{jsonFileName}\"";
            
            return File(jsonStream, "application/octet-stream");
        }

        var exportStream = await elementExportService.ExportMultipleRowsAsync(selectedRows);
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var zipFileName = $"DataDictionaryExportation_{timestamp}.zip";

        Response.Headers.ContentDisposition  = $"attachment; filename=\"{zipFileName}\"";
        
        return File(exportStream, "application/octet-stream");
    }

    public async Task<IActionResult> Import()
    {
        var uploadArea = uploadAreaFactory.Create();
        
        ConfigureUploadArea(uploadArea);
        
        var result = await uploadArea.GetResultAsync();

        if (result is IActionResult actionResult)
            return actionResult;
        
        return PartialView(new ImportViewModel(result.Content));
    }

    private void ConfigureUploadArea(JJUploadArea upload)
    {
        upload.AllowedTypes = "json,zip";
        upload.JsCallback = "importationCallback()";
        upload.Url = Url.Action("Import", "Element");
        upload.OnFileUploadedAsync += FileUploaded;
    }

    private async ValueTask FileUploaded(object? sender, FormUploadFileEventArgs e)
    {
        await using var ms = new MemoryStream(e.File.Bytes);
        if (e.File.FileName.EndsWith(".zip"))
        {
            await elementImportService.ImportZipFile(ms);
        }
        else
        {
            await elementImportService.Import(ms);
        }
 
        if (ModelState.IsValid)
        {
            e.SuccessMessage = stringLocalizer["Dictionary imported successfully!"];
        }
        else
        {
            var jjSummary = elementService.GetValidationSummary();
            foreach (var err in jjSummary.Errors)
                e.ErrorMessage += $"<br>{err}";
        }
    }

    public ViewResult Duplicate(string? elementName = null)
    {
        return View(new DuplicateElementViewModel{ OriginalElementName = elementName });
    }

    public async ValueTask<ViewResult> Class(string elementName)
    {
        ViewData["Class"] = await classGenerationService.GetClassSourceCode(elementName);
        ViewData["ElementName"] = elementName;

        return View("Class");
    }

    public async Task<PartialViewResult> Scripts(string elementName)
    {
        var formElement = await elementService.GetFormElementAsync(elementName);
        var scripts = await scriptsService.GetScriptsAsync(formElement);
        var tableExists = await entityRepository.TableExistsAsync(formElement.Schema, formElement.TableName, formElement.ConnectionId);
        
        var model = new ElementScriptsViewModel
        {
            ElementName = elementName,
            Scripts = scripts,
            TableExists = tableExists
        };

        return PartialView("_Scripts", model);
    }

    [HttpPost]
    public async Task<IActionResult> Add(ElementBean model)
    {
        var formElement = await elementService.CreateEntityAsync(model);
        if (formElement != null)
        {
            return RedirectToAction("Index", "Entity", new { elementName = formElement.Name });
        }

        var validationSummary = elementService.GetValidationSummary();
        ViewBag.Error = validationSummary.GetHtml();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Duplicate(DuplicateElementViewModel model)
    {
        JJValidationSummary validationSummary;
        if (!ModelState.IsValid)
        {
            validationSummary = elementService.GetValidationSummary();
            ViewBag.Error = validationSummary.GetHtml();
        }
            
        if (await elementService.DuplicateEntityAsync(model.OriginalElementName!, model.NewElementName!))
            return RedirectToAction("Index", new { elementName = model.NewElementName });

        validationSummary = elementService.GetValidationSummary();
        ViewBag.Error = validationSummary.GetHtml();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Scripts(string elementName, string scriptOption)
    {
        try
        {
            await scriptsService.ExecuteScriptsAsync(elementName, scriptOption);
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            var error = new { success = false, message = ex.Message };
            return new JsonResult(error) { StatusCode = (int)HttpStatusCode.InternalServerError };
        }
    }
    
    public async Task<IActionResult> Delete()
    {
        var formView = elementService.GetFormView();
        var selectedGridValues = formView.GridView.GetSelectedGridValues();

        var elementNamesToDelete = selectedGridValues
            .Where(value => value.TryGetValue("name", out var nameValue) && nameValue is string)
            .Select(value => value["name"].ToString());

        foreach (var elementName in elementNamesToDelete)
        {
            await elementService.DeleteAsync(elementName);
        }

        return RedirectToAction(nameof(Index));
    }

    
}