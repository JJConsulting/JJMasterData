#nullable enable

using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

[Area("DataDictionary")]
public class FieldController : DataDictionaryController
{
    private readonly FieldService _fieldService;

    public FieldController(FieldService fieldService)
    {
        _fieldService = fieldService;
    }

    public IActionResult Index(string dictionaryName, string? fieldName)
    {
        var formElement = _fieldService.GetFormElement(dictionaryName);
        FormElementField? field;
        if (string.IsNullOrEmpty(fieldName))
        {
            if (TempData.ContainsKey("field"))
                field = TempData.Get<FormElementField>("field");
            else if (formElement.Fields.Count > 0)
                field = formElement.Fields[0];
            else
                field = new FormElementField();
        }
        else
        {
            field = formElement.Fields[fieldName];
        }

        PopulateViewBag(formElement, field);
        return View(nameof(Index), field);
    }

    //Partial View
    public IActionResult Detail(string dictionaryName, string fieldName)
    {
        var formElement = _fieldService.GetFormElement(dictionaryName);
        var field = formElement.Fields[fieldName];
        PopulateViewBag(formElement, field);
        return PartialView("_Detail", field);
    }

    public IActionResult Add(string dictionaryName)
    {
        var formElement = _fieldService.GetFormElement(dictionaryName);
        var field = new FormElementField();
        PopulateViewBag(formElement, field);
        return PartialView("_Detail", field);
    }

    public IActionResult Icons(string dictionaryName, string itemId)
    {
        ViewBag.ItemId = itemId;
        return View("Icons");
    }

    public IActionResult Delete(string dictionaryName, string fieldName)
    {
        _fieldService.DeleteField(dictionaryName, fieldName);
        var nextField = _fieldService.GetNextFieldName(dictionaryName, fieldName);
        return RedirectToAction("Index", new { dictionaryName, fieldName = nextField });
    }

    [HttpPost]
    public IActionResult Save(string dictionaryName, FormElementField field, string? originalName)
    {
        RecoverCustomAttibutes(ref field);
        _fieldService.SaveField(dictionaryName, field, originalName);
        if (ModelState.IsValid)
        {
            return RedirectToIndex(dictionaryName, field);
        }

        ViewBag.Error = _fieldService.GetValidationSummary().GetHtml();
        return RedirectToIndex(dictionaryName, field);
    }

    [HttpPost]
    public IActionResult Index(string dictionaryName, FormElementField? field)
    {
        var formElement = _fieldService.GetFormElement(dictionaryName);
        PopulateViewBag(formElement, field);
        return View("Index", field);
    }


    [HttpPost]
    public IActionResult Sort(string dictionaryName, string[] orderFields)
    {
        _fieldService.SortFields(dictionaryName, orderFields);
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult Copy(string dictionaryName, FormElementField? field)
    {
        var dictionary = _fieldService.DictionaryRepository.GetMetadata(dictionaryName);
        _fieldService.CopyField(dictionary, field);
        if (!ModelState.IsValid)
            ViewBag.Error = _fieldService.GetValidationSummary().GetHtml();

        var formElement = dictionary.GetFormElement();
        PopulateViewBag(formElement, field);
        return View("Index", field);
    }

    [HttpPost]
    public IActionResult AddDataItem(string dictionaryName, FormElementField field, int qtdRowsToAdd)
    {
        for (int i = 0; i < qtdRowsToAdd; i++)
        {
            var item = new DataItemValue();
            item.Id = field.DataItem.Items.Count.ToString();
            item.Description = "";
            item.Icon = IconType.Star;
            item.ImageColor = "#ffffff";
            field.DataItem.Items.Add(item);
        }
        return RedirectToIndex(dictionaryName, field);
    }

    [HttpPost]
    public IActionResult RemoveDataItem(string dictionaryName, FormElementField field, int dataItemIndex)
    {
        field.DataItem.Items.RemoveAt(dataItemIndex);
        return RedirectToIndex(dictionaryName, field);
    }

    [HttpPost]
    public IActionResult RemoveAllDataItem(string dictionaryName, FormElementField field)
    {
        field.DataItem.Items = new List<DataItemValue>();
        return RedirectToIndex(dictionaryName, field);
    }

    [HttpPost]
    public IActionResult AddElementMapFilter(string dictionaryName, FormElementField field, string mapField, string mapExpressionValue)
    {
        var mapFilter = new DataElementMapFilter
        {
            FieldName = mapField,
            ExpressionValue = mapExpressionValue
        };

        bool isValid = _fieldService.AddElementMapFilter(field, mapFilter);
        if (!isValid)
        {
            ViewBag.Error = _fieldService.GetValidationSummary().GetHtml();
        }

        return RedirectToIndex(dictionaryName, field);
    }

    [HttpPost]
    public IActionResult RemoveElementMapFilter(string dictionaryName, FormElementField field, string elementMapFieldName)
    {
        field.DataItem.ElementMap.MapFilters.RemoveAll(x => x.FieldName.Equals(elementMapFieldName));
        return RedirectToIndex(dictionaryName, field);
    }

    private IActionResult RedirectToIndex(string dictionaryName, FormElementField field)
    {
        TempData.Put("field",field);
        TempData["error"] = ViewBag.Error;
        TempData["selected_tab"] = Request.Form["selected_tab"].ToString();
        TempData["originalName"] = Request.Form["originalName"].ToString();

        return RedirectToAction("Index", new { dictionaryName });
    }

    private void PopulateViewBag(FormElement formElement, FormElementField? field)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));
        if (field == null)
            throw new ArgumentNullException(nameof(field));
        
        field.DataItem ??= new FormElementDataItem();

        field.DataItem.ElementMap ??= new DataElementMap();

        field.DataItem.ElementMap.MapFilters ??= new List<DataElementMapFilter>();
        
        field.DataFile ??= new FormElementDataFile
        {
            MaxFileSize = 2097152 //2mb
        };

        field.DataFile.MaxFileSize /= 1000000;

        //Refresh action
        if (formElement.Fields.Contains(field.Name))
            field.Actions = formElement.Fields[field.Name].Actions;

        if (!string.IsNullOrEmpty(Request.Query["selected_tab"]))
            ViewBag.Tab = Request.Form["selected_tab"].ToString();
        else if (TempData["selected_tab"] != null)
            ViewBag.Tab = TempData["selected_tab"]!;

        if (TempData.ContainsKey("error"))
            ViewBag.Error = TempData["error"]!;

        if ((string?)Request.Query["originalName"] != null)
            ViewBag.OriginalName = Request.Form["originalName"].ToString();
        else if (TempData["originalName"] != null)
            ViewBag.OriginalName = TempData["originalName"]!;
        else
            ViewBag.OriginalName = field.Name;

        ViewBag.MenuId = "Field";
        ViewBag.DictionaryName = formElement.Name;
        ViewBag.HintDictionary = _fieldService.GetHintDictionary(formElement);
        ViewBag.MaxRequestLength = GetMaxRequestLength();
        ViewBag.FieldName = field.Name;
        ViewBag.Fields = formElement.Fields;

        if (field.Component != FormComponent.Lookup) return;
        
        ViewBag.ElementNameList = _fieldService.GetElementList();
        ViewBag.ElementFieldList = _fieldService.GetElementFieldList(field);
    }
    private void RecoverCustomAttibutes(ref FormElementField field)
    {
        field.Attributes = new Hashtable();
        if (field.Component == FormComponent.Text |
            field.Component == FormComponent.Number |
            field.Component == FormComponent.Password |
            field.Component == FormComponent.Email |
            field.Component == FormComponent.Cnpj |
            field.Component == FormComponent.Cpf |
            field.Component == FormComponent.CnpjCpf |
            field.Component == FormComponent.Cep)
        {
            field.SetAttr(FormElementField.PlaceholderAttribute, Request.Form["txtPlaceHolder"].ToString());
        }
        else if (field.Component == FormComponent.TextArea)
        {
            field.SetAttr(FormElementField.RowsAttribute, Request.Form["txtTextAreaRows"].ToString());
        }
        else if (field.Component == FormComponent.Lookup)
        {
            field.SetAttr(FormElementField.PopUpSizeAttribute, Request.Form["cboLkPopUpSize"].ToString());
            field.SetAttr(FormElementField.PopUpTitleAttribute, Request.Form["txtLkPopUpTitle"].ToString());
        }
    }

    public int GetMaxRequestLength()
    {
        // ASP.NET Core enforces 30MB (~28.6 MiB) max request body size limit, be it Kestrel and HttpSys.
        // Under normal circumstances, there is no need to increase the size of the HTTP request.

        int maxRequestLength = 30720000;

        return maxRequestLength;
    }

}