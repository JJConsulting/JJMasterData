using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Filters;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class RulesController(FormElementRulesService formElementRulesService) : DataDictionaryController
{
    [ImportModelState]
    public async Task<IActionResult> Index(string elementName, int? ruleId = null)
    {
        var formElement = await formElementRulesService.GetFormElementAsync(elementName);
        FormElementRule rule;

        if (ruleId == null)
        {
            if (TempData.ContainsKey("rule"))
                rule = TempData.Get<FormElementRule>("rule")!;
            else if (formElement.Rules.Count > 0)
                rule = formElement.Rules[0];
            else
                rule = new FormElementRule();
        }
        else
        {
            rule = formElement.GetRuleById(ruleId.Value);
        }

        PopulateViewData(formElement, rule);
        return View(rule);
    }

    public async Task<IActionResult> Detail(string elementName, int ruleId)
    {
        var formElement = await formElementRulesService.GetFormElementAsync(elementName);
        var rule = formElement.GetRuleById(ruleId);
        PopulateViewData(formElement, rule);
        return PartialView("_Detail", rule);
    }

    public async Task<IActionResult> Add(string elementName)
    {
        var formElement = await formElementRulesService.GetFormElementAsync(elementName);
        var rule = new FormElementRule();
        PopulateViewData(formElement, rule);
        return PartialView("_Detail", rule);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeLanguage(string elementName, FormElementRule rule)
    {
        var formElement = await formElementRulesService.GetFormElementAsync(elementName);
        PopulateViewData(formElement, rule);
        return PartialView("_Detail", rule);
    }

    [HttpPost]
    [ExportModelState]
    public async Task<IActionResult> Save(string elementName, FormElementRule rule, string selectedTab = "#div-general")
    {
        await formElementRulesService.SaveAsync(elementName, rule);
        if (ModelState.IsValid)
            return RedirectToAction("Index", new { elementName, ruleId = rule.Id, selectedTab });

        return RedirectToIndex(elementName, rule, selectedTab);
    }

    public async Task<IActionResult> Delete(string elementName, int ruleId)
    {
        await formElementRulesService.DeleteAsync(elementName, ruleId);
        return RedirectToAction("Index", new { elementName });
    }

    [HttpPost]
    public async Task<IActionResult> Index(string elementName, FormElementRule rule)
    {
        var formElement = await formElementRulesService.GetFormElementAsync(elementName);
        PopulateViewData(formElement, rule);
        return View(rule);
    }

    private RedirectToActionResult RedirectToIndex(string elementName, FormElementRule rule, string selectedTab)
    {
        TempData.Put("rule", rule);

        return RedirectToAction("Index", new { elementName, selectedTab });
    }

    private void PopulateViewData(FormElement formElement, FormElementRule rule)
    {
        var selectedTab = Request.HasFormContentType
            ? Request.Form["selectedTab"].ToString()
            : Request.Query["selectedTab"].ToString();

        ViewBag.Tab = string.IsNullOrWhiteSpace(selectedTab) ? "#div-general" : selectedTab;
        ViewData["MenuId"] = "Rules";
        ViewData["ElementName"] = formElement.Name;
        ViewData["CodeEditorHints"] = formElement.Fields.Select(f => new CodeEditorHint
        {
            Language = "sql",
            InsertText = f.Name,
            Label = f.Name,
            Details = "Form Element Field",
        })
        .Concat(
            formElement.Fields.Select(f => new CodeEditorHint
            {
                Language = "javascript",
                InsertText = $"values.{f.Name}",
                Label = $"values.{f.Name}",
                Details = "Form Element Field",
            }))
        .Concat(
        [
            new CodeEditorHint
            {
                Language = "javascript",
                InsertText = "addError('Mensagem de erro');",
                Label = "addError(message)",
                Details = "Adds a general error message",
            },
            new CodeEditorHint
            {
                Language = "javascript",
                InsertText = "addError('FieldName', 'Mensagem de erro');",
                Label = "addError(name, message)",
                Details = "Adds a field error",
            },
        ])
        .ToList();
        ViewBag.RuleId = rule.Id;
        ViewBag.Rules = formElement.Rules;
    }
}
