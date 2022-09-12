using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Controllers;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

[Area("DataDictionary")]
public class PanelController : DataDictionaryController
{
    private readonly PanelService _panelService;

    public PanelController(PanelService panelService)
    {
        _panelService = panelService;
    }

    public ActionResult Index(string dictionaryName, int? panelId = null)
    {
        var formElement = _panelService.GetFormElement(dictionaryName);
        FormElementPanel panel;
        if (panelId == null)
        {
            if (TempData.ContainsKey("panel"))
                panel = TempData.Get<FormElementPanel>("panel");
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
    public IActionResult Detail(string dictionaryName, int panelId)
    {
        var formElement = _panelService.GetFormElement(dictionaryName);
        var panel = formElement.GetPanelById(panelId);
        PopulateViewBag(formElement, panel);
        return PartialView("_Detail", panel);
    }

    //Partial View
    public IActionResult Add(string dictionaryName)
    {
        var formElement = _panelService.GetFormElement(dictionaryName);
        var panel = new FormElementPanel();
        PopulateViewBag(formElement, panel);
        return PartialView("_Detail", panel);
    }

    public IActionResult Delete(string dictionaryName, int panelId)
    {
        var formElement = _panelService.GetFormElement(dictionaryName);
        _panelService.DeleteField(formElement, panelId);

        return RedirectToAction("Index", new { dictionaryName });
    }

    [HttpPost]
    public IActionResult Save(string dictionaryName, FormElementPanel panel, [FromForm] string? selectedFields)
    {
        var formElement = _panelService.GetFormElement(dictionaryName);
        var selectedFormElementFields = GetFormFields(formElement, panel, selectedFields);

        _panelService.SavePanel(formElement, panel, selectedFormElementFields);
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index", new { dictionaryName, panelId = panel.PanelId });
        }

        ViewBag.Error = _panelService.GetValidationSummary().GetHtml();
        return RedirectToIndex(dictionaryName, panel);
    }

    [HttpPost]
    public IActionResult Index(string dictionaryName, FormElementPanel panel)
    {
        var formElement = _panelService.GetFormElement(dictionaryName);
        PopulateViewBag(formElement, panel);
        return View("Index", panel);
    }

    [HttpPost]
    public IActionResult Sort(string dictionaryName, string[] orderFields)
    {
        _panelService.SortPanels(dictionaryName, orderFields);
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult Copy(string dictionaryName, FormElementPanel panel)
    {
        var formElement = _panelService.GetFormElement(dictionaryName);
        var newPanel = panel.DeepCopy();
        newPanel.PanelId = 1 + formElement.Panels.Max(x => x.PanelId);
        formElement.Panels.Add(newPanel);
        _panelService.DicDao.SetFormElement(formElement);
        return RedirectToIndex(dictionaryName, newPanel);
    }

    private IActionResult RedirectToIndex(string dictionaryName, FormElementPanel panel)
    {
        TempData.Put("panel",panel);
        TempData["error"] = ViewBag.Error;
        TempData["selected_tab"] = Request.Form["selected_tab"];

        return RedirectToAction("Index", new { dictionaryName });
    }

    private void PopulateViewBag(FormElement formElement, FormElementPanel panel, string? selectedFields = null)
    {
        if (!string.IsNullOrEmpty(Request.Query["selected_tab"]))
            ViewBag.Tab = Request.Form["selected_tab"];
        else if (TempData["selected_tab"] != null)
            ViewBag.Tab = TempData["selected_tab"];

        if (TempData.ContainsKey("error"))
            ViewBag.Error = TempData["error"];

        ViewBag.MenuId = "Panel";
        ViewBag.DictionaryName = formElement.Name;
        ViewBag.PanelId = panel.PanelId;
        ViewBag.Panels = formElement.Panels;
        ViewBag.AvailableFields = GetAvailableFields(formElement, panel);
        ViewBag.SelectedFields = GetFormFields(formElement, panel, selectedFields);
    }

    protected List<FormElementField> GetAvailableFields(FormElement formElement, FormElementPanel panel)
    {
        var list = new List<FormElementField>();
        if ((string)Request.Query["enabled_fields"] == null)
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

    protected List<FormElementField> GetFormFields(FormElement formElement, FormElementPanel panel,
        string? selectedFields)
    {
        var list = new List<FormElementField>();
        if (selectedFields == null)
        {
            if (panel.PanelId > 0)
            {
                list = formElement.Fields.ToList().FindAll(x => x.PanelId == panel.PanelId);
            }
        }
        else
        {
            string[] splittedFields = selectedFields.Split(',');
            foreach (string fieldName in splittedFields)
            {
                if (formElement.Fields.Contains(fieldName))
                    list.Add(formElement.Fields[fieldName]);
            }
        }

        return list;
    }
}