using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class PanelController(PanelService panelService) : DataDictionaryController
{
    public async Task<ActionResult> Index(string elementName, int? panelId = null)
    {
        var formElement = await panelService.GetFormElementAsync(elementName);
        FormElementPanel panel;
        if (panelId == null)
        {
            if (TempData.ContainsKey("panel"))
                panel = TempData.Get<FormElementPanel>("panel")!;
            else if (formElement.Panels.Count > 0)
                panel = formElement.Panels[0];
            else
                panel = new FormElementPanel();
        }
        else
        {
            panel = formElement.GetPanelById((int)panelId);
        }

        PopulateViewBag(formElement, panel);
        return View("Index", panel);
    }

    //Partial View
    public async Task<IActionResult> Detail(string elementName, int panelId)
    {
        var formElement = await panelService.GetFormElementAsync(elementName);
        var panel = formElement.GetPanelById(panelId);
        PopulateViewBag(formElement, panel);
        return PartialView("_Detail", panel);
    }

    //Partial View
    public async Task<IActionResult> Add(string elementName)
    {
        var formElement =await  panelService.GetFormElementAsync(elementName);
        var panel = new FormElementPanel();
        PopulateViewBag(formElement, panel);
        return PartialView("_Detail", panel);
    }

    public async Task<IActionResult> Delete(string elementName, int panelId)
    {
        await panelService.DeleteFieldAsync(elementName, panelId);
        return RedirectToAction("Index", new { elementName });
    }

    [HttpPost]
    public async Task<IActionResult> Save(string elementName, FormElementPanel panel, [FromForm] string? selectedFields)
    {
        string[]? splittedFields = selectedFields?.Split(',');
        await panelService.SavePanelAsync(elementName, panel, splittedFields);
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index", new { elementName, panelId = panel.PanelId });
        }

        ViewBag.Error = panelService.GetValidationSummary().GetHtml();
        return RedirectToIndex(elementName, panel);
    }

    [HttpPost]
    public async Task<IActionResult> Index(string elementName, FormElementPanel panel)
    {
        var formElement = await panelService.GetFormElementAsync(elementName);
        PopulateViewBag(formElement, panel);
        return View("Index", panel);
    }

    [HttpPost]
    public async Task<IActionResult> Sort(string elementName, string[] orderFields)
    {
        await panelService.SortPanelsAsync(elementName, orderFields);
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Copy(string elementName, FormElementPanel panel)
    {
        var newPanel = await panelService.CopyPanel(elementName, panel);
        return RedirectToIndex(elementName, newPanel);
    }

    private RedirectToActionResult RedirectToIndex(string elementName, FormElementPanel panel)
    {
        TempData.Put("panel",panel);
        TempData["error"] = ViewBag.Error;
        TempData["selected-tab"] = Request.Form["selected-tab"];

        return RedirectToAction("Index", new { elementName });
    }

    private void PopulateViewBag(FormElement formElement, FormElementPanel panel)
    {
        if (Request.HasFormContentType && Request.Form.TryGetValue("selected-tab", out var selectedTab)) 
            ViewBag.Tab = selectedTab;

        else if (TempData.TryGetValue("selected-tab",  out var tempSelectedTab))
            ViewBag.Tab = tempSelectedTab?.ToString()!;

        if (TempData.ContainsKey("error"))
            ViewBag.Error = TempData["error"]!;

        ViewBag.MenuId = "Panels";
        ViewBag.ElementName = formElement.Name;
        ViewBag.PanelId = panel.PanelId;
        ViewBag.Panels = formElement.Panels;
        ViewBag.AvailableFields = GetAvailableFields(formElement, panel);
        ViewBag.CodeMirrorHintList = JsonConvert.SerializeObject(panelService.GetAutocompleteHintsList(formElement));
        ViewBag.SelectedFields = (panel.PanelId > 0) ?
            formElement.Fields.ToList().FindAll(x => x.PanelId == panel.PanelId) : [];
    }

    protected List<FormElementField> GetAvailableFields(FormElement formElement, FormElementPanel panel)
    {
        var list = new List<FormElementField>();
        if ((string?)Request.Query["enabled_fields"] == null)
        {
            list = formElement.Fields.ToList().FindAll(x => x.PanelId == 0);
        }
        else
        {
            string[] availableFields = Request.Form["enabled_fields"].ToString().Split(',');
            foreach (string fieldName in availableFields)
            {
                if (formElement.Fields.Contains(fieldName))
                    list.Add(formElement.Fields[fieldName]);
            }
        }

        return list;
    }

}