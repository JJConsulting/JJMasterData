#nullable enable

using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class FieldController(FieldService fieldService, IControlFactory<JJSearchBox> searchBoxFactory)
    : DataDictionaryController
{
    public async Task<IActionResult> Index(string elementName, string? fieldName)
    {
        var formElement = await fieldService.GetFormElementAsync(elementName);
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

        await PopulateViewBag(formElement, field);
        return View(nameof(Index), field);
    }
    
    public async Task<IActionResult> Detail(string elementName, string fieldName)
    {
        var formElement = await fieldService.GetFormElementAsync(elementName);
        var field = formElement.Fields[fieldName];
        await PopulateViewBag(formElement, field);
        return PartialView("_Detail", field);
    }

    public async Task<IActionResult> Add(string elementName)
    {
        var formElement = await fieldService.GetFormElementAsync(elementName);
        var field = new FormElementField();
        await PopulateViewBag(formElement, field);
        return PartialView("_Detail", field);
    }
    
    public async Task<IActionResult> Delete(string elementName, string fieldName)
    {
        await fieldService.DeleteField(elementName, fieldName);
        var nextField = await fieldService.GetNextFieldNameAsync(elementName, fieldName);
        return RedirectToAction("Index", new { elementName, fieldName = nextField });
    }
    
    [HttpPost]
    public async Task<IActionResult> Index(string elementName, string? fieldName, FormElementField? field)
    {
        
        var iconSearchBox = searchBoxFactory.Create();
        iconSearchBox.Name = fieldName ?? "searchBox";
        iconSearchBox.DataItem.ShowIcon = true;
        iconSearchBox.DataItem.Items = Enum.GetValues<IconType>()
            .Select(i => new DataItemValue(i.GetId().ToString(), i.GetDescription(), i, "6a6a6a")).ToList();

        var iconSearchBoxResult = await iconSearchBox.GetResultAsync();

        if (iconSearchBoxResult is IActionResult actionResult)
            return actionResult;
        
        var formElement = await fieldService.GetFormElementAsync(elementName);
        await PopulateViewBag(formElement, field);
        return View("Index", field);
    }

    [HttpPost]
    public async Task<IActionResult> Save(string elementName, FormElementField field, string? originalName)
    {
        RecoverCustomAttibutes(ref field);
        
        await fieldService.SaveFieldAsync(elementName, field, originalName);
        if (ModelState.IsValid)
        {
            return RedirectToIndex(elementName, field);
        }

        ViewBag.Error = fieldService.GetValidationSummary().GetHtml();
        return RedirectToIndex(elementName, field);
    }


    [HttpPost]
    public async Task<IActionResult> Sort(string elementName, string fieldsOrder)
    {
        await fieldService.SortFieldsAsync(elementName, fieldsOrder.Split(","));
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Copy(string elementName, FormElementField? field)
    {
        var dictionary = await  fieldService.DataDictionaryRepository.GetFormElementAsync(elementName);
        await fieldService.CopyFieldAsync(dictionary, field);
        if (!ModelState.IsValid)
            ViewBag.Error = fieldService.GetValidationSummary().GetHtml();

        await PopulateViewBag(dictionary, field);
        return View("Index", field);
    }

    [HttpPost]
    public IActionResult AddDataItem(string elementName, FormElementField field, int qtdRowsToAdd)
    {
        field.DataItem ??= new FormElementDataItem();
        field.DataItem.Items ??= new List<DataItemValue>();
        for (int i = 0; i < qtdRowsToAdd; i++)
        {
            var item = new DataItemValue
            {
                Id = field.DataItem.Items.Count.ToString(),
                Description = "",
                Icon = IconType.Star,
                IconColor = "#ffffff"
            };
            field.DataItem.Items.Add(item);
        }
        return RedirectToIndex(elementName, field);
    }

    [HttpPost]
    public IActionResult RemoveDataItem(string elementName, FormElementField field, int dataItemIndex)
    {
        field.DataItem?.Items?.RemoveAt(dataItemIndex);
        return RedirectToIndex(elementName, field);
    }

    [HttpPost]
    public IActionResult RemoveAllDataItem(string elementName, FormElementField field)
    {
        field.DataItem!.Items = new List<DataItemValue>();
        return RedirectToIndex(elementName, field);
    }

    [HttpPost]
    public async Task<IActionResult> AddElementMapFilter(string elementName, FormElementField field, string mapField, [BooleanExpression] string mapExpressionValue)
    {
        var elementMapFilter = new DataElementMapFilter
        {
            FieldName = mapField,
            ExpressionValue = mapExpressionValue
        };

        bool isValid = await fieldService.AddElementMapFilterAsync(field, elementMapFilter);
        if (!isValid)
        {
            ViewBag.Error = fieldService.GetValidationSummary().GetHtml();
        }

        return RedirectToIndex(elementName, field);
    }

    [HttpPost]
    public IActionResult RemoveElementMapFilter(string elementName, FormElementField field, string elementMapFieldName)
    {
        field.DataItem!.ElementMap!.MapFilters.RemoveAll(x => x.FieldName.Equals(elementMapFieldName));
        return RedirectToIndex(elementName, field);
    }

    private RedirectToActionResult RedirectToIndex(string elementName, FormElementField field)
    {
        TempData.Put("field", field);
        TempData["error"] = ViewBag.Error;
        TempData["selected-tab"] = Request.Form["selected-tab"].ToString();
        TempData["originalName"] = Request.Form["originalName"].ToString();

        return RedirectToAction("Index", new { elementName });
    }

    private async Task PopulateViewBag(FormElement formElement, FormElementField? field)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        field.DataItem ??= new FormElementDataItem();
        field.DataFile ??= new FormElementDataFile
        {
            MaxFileSize = 2097152 //2mb
        };
        
        //Refresh action
        if (formElement.Fields.Contains(field.Name))
            field.Actions = formElement.Fields[field.Name].Actions;

        if (Request.HasFormContentType && Request.Form.TryGetValue("selected-tab", out var selectedTab)) 
            ViewBag.Tab = selectedTab;

        else if (TempData.TryGetValue("selected-tab",  out var tempSelectedTab))
            ViewBag.Tab = tempSelectedTab?.ToString()!;
        
        if (TempData.TryGetValue("error", out var value))
            ViewBag.Error = value!;

        if (Request.HasFormContentType && Request.Form.TryGetValue("originalName", out var originalName))
            ViewBag.OriginalName = originalName;
        else if (TempData.TryGetValue("originalName",  out var tempOriginalName))
            ViewBag.OriginalName = tempOriginalName?.ToString()!;
        
        if (TempData["originalName"] != null)
            ViewBag.OriginalName = TempData["originalName"]!;
        else
            ViewBag.OriginalName = field.Name;

        ViewBag.MenuId = "Field";
        ViewBag.FormElement = formElement;
        ViewBag.ElementName = formElement.Name;
        ViewBag.CodeMirrorHintList = JsonConvert.SerializeObject(fieldService.GetAutocompleteHintsList(formElement));
        ViewBag.MaxRequestLength = GetMaxRequestLength();
        ViewBag.FieldName = field.Name;
        ViewBag.Fields = formElement.Fields;

        if (field.Component is not FormComponent.Lookup && field.Component is not FormComponent.Search && field.Component is not FormComponent.ComboBox) 
            return;

        ViewBag.ElementNameList = await fieldService.GetElementListAsync();

        field.DataItem.ElementMap ??= new DataElementMap();
        ViewBag.ElementFieldList = await fieldService.GetElementFieldListAsync(field.DataItem.ElementMap);
    }
    private void RecoverCustomAttibutes(ref FormElementField field)
    {
        field.Attributes = new Dictionary<string, object>();
        switch (field.Component)
        {
            case FormComponent.Text:
            case FormComponent.Number:
            case FormComponent.Password:
            case FormComponent.Email:
            case FormComponent.Cnpj:
            case FormComponent.Cpf:
            case FormComponent.Slider:
            case FormComponent.CnpjCpf:
            case FormComponent.Cep:
                field.SetAttr(FormElementField.PlaceholderAttribute, Request.Form["txtPlaceHolder"].ToString());

                if (field.Component is FormComponent.Number or FormComponent.Slider)
                {
                    if (double.TryParse(Request.Form["step"], out var step))
                    {
                        field.SetAttr(FormElementField.StepAttribute, step);
                    }
                    if (double.TryParse(Request.Form["minValue"], out var minValue))
                    {
                        field.SetAttr(FormElementField.MinValueAttribute, minValue);
                    }
                    if (double.TryParse(Request.Form["maxValue"], out var maxValue))
                    {
                        field.SetAttr(FormElementField.MaxValueAttribute, maxValue);
                    }
                }

                break;
            case FormComponent.TextArea:
                field.SetAttr(FormElementField.RowsAttribute, Request.Form["txtTextAreaRows"].ToString());
                break;
            case FormComponent.ComboBox:
            case FormComponent.Search:
            case FormComponent.Lookup:
                field.SetAttr(FormElementField.PopUpSizeAttribute, Request.Form["cboLkPopUpSize"].ToString());
                field.SetAttr(FormElementField.PopUpTitleAttribute, Request.Form["txtLkPopUpTitle"].ToString());
                break;
            case FormComponent.CheckBox:
                field.SetAttr(FormElementField.IsSwitchAttribute, Request.Form[FormElementField.IsSwitchAttribute] == "true");
                break;
            case FormComponent.Date
                or FormComponent.DateTime
                or FormComponent.Hour:
                field.SetAttr(FormElementField.AutocompletePickerAttribute, Request.Form[FormElementField.AutocompletePickerAttribute] == "true");
                field.SetAttr(FormElementField.MultipleDates, Request.Form[FormElementField.MultipleDates] == "true");
                break;

        }
    }

    public int GetMaxRequestLength()
    {
        // ASP.NET Core enforces 30MB (~28.6 MiB) max request body size limit, be it Kestrel and HttpSys.
        // Under normal circumstances, there is no need to increase the size of the HTTP request.

        const int maxRequestLength = 30720000;

        return maxRequestLength;
    }

}