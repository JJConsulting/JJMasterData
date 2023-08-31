#nullable enable
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components.Abstractions;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Factories;

namespace JJMasterData.Core.Web.Components;

/// Represents a field with a value from another Data Dictionary accessed via popup.
public class JJLookup : AsyncControl
{
    internal FormElement FormElement { get; set; }
    private ILookupService LookupService { get; }
    private IEncryptionService EncryptionService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }

    #region "Properties"

    private object? _selectedValue;
    private string? _text;

    internal FormStateData FormStateData { get; set; }

    public bool AutoReloadFormFields { get; set; }

    public bool OnlyNumbers { get; set; }

    public ModalSize ModalSize
    {
        get
        {
            string val = GetAttr("popupsize");
            if (string.IsNullOrEmpty(val))
                return ModalSize.Large;
            return (ModalSize)int.Parse(val);
        }
        set => SetAttr("popupsize", value);
    }

    public string PopTitle
    {
        get => GetAttr("popuptitle");
        set => SetAttr("popuptitle", value);
    }

    public new string? Text
    {
        get
        {
            if (AutoReloadFormFields && _text == null && CurrentContext.IsPost)
            {
                _text = CurrentContext.Request[Name];
            }

            return _text;
        }
        set => _text = value;
    }

    public object? SelectedValue
    {
        get
        {
            if (AutoReloadFormFields && string.IsNullOrEmpty(_selectedValue?.ToString()))
            {
                _selectedValue = LookupService.GetSelectedValue(Name);
            }

            return _selectedValue;
        }
        set => _selectedValue = value;
    }


    public DataElementMap ElementMap { get; }
    
    public string FieldName { get; }
    #endregion

    #region "Constructors"

    internal JJLookup(
        FormElement formElement,
        FormElementField field,
        ControlContext controlContext,
        IHttpContext httpContext,
        ILookupService lookupService,
        IEncryptionService encryptionService,
        JJMasterDataUrlHelper urlHelper) : base(httpContext)
    {
        FormElement = formElement;
        ElementMap = field.DataItem?.ElementMap ?? throw new ArgumentException("ElementMap cannot be null.");
        FieldName = field.Name;
        LookupService = lookupService;
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
        Enabled = true;
        AutoReloadFormFields = true;
        Name = field.Name;
        ModalSize = ModalSize.Large;
        PopTitle = "Search";
        FormStateData = controlContext.FormStateData;
        UserValues = controlContext.FormStateData.UserValues;
        SelectedValue = controlContext.Value?.ToString();
        SetAttr(field.Attributes);
        SetAttr("panelName", controlContext.ParentComponentName);

        if (field.DataType is FieldType.Int)
        {
            OnlyNumbers = true;
            MaxLength = 11;
        }
        else
        {
            MaxLength = field.Size;
        }
        
    }

    #endregion


    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (!IsLookupRoute())
            return new RenderedComponentResult(await GetLookupHtml());

        if (IsAjaxGetDescription())
            return new JsonComponentResult(await GetResultAsync());
        
        return new EmptyComponentResult();
    }

    private async Task<HtmlBuilder> GetLookupHtml()
    {
        object? inputValue = SelectedValue;
        string? description = Text;

        if (string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(inputValue?.ToString()))
            description = await LookupService.GetDescriptionAsync(ElementMap, FormStateData, inputValue?.ToString(), OnlyNumbers);

        Attributes["lookup-url"] = LookupService.GetLookupUrl(ElementMap, FormStateData, Name);
        Attributes["lookup-field-name"] = FieldName;
        
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass("input-group mb-3 d-flex" );
        var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(FormElement.Name);

        Attributes["lookup-result-url"] = UrlHelper.GetUrl("GetResult", "Lookup","MasterData", 
            new
            {
                dictionaryName = encryptedDictionaryName,
                componentName = Name,
                fieldName = FieldName,
                pageState = FormStateData.PageState
            });
        
        var idTextBox = new JJTextBox(CurrentContext)
        {
            Name = Name,
            CssClass = $"form-control jjlookup {CssClass}",
            InputType = OnlyNumbers ? InputType.Number : InputType.Text,
            MaxLength = MaxLength,
            Text = SelectedValue?.ToString(),
            Attributes = Attributes.DeepCopy(),
            ToolTip = ToolTip,
            ReadOnly = ReadOnly, 
            Enabled = Enabled,
        };

        idTextBox.Attributes["style"] = "flex:2";

        var descriptionTextBox = new JJTextBox(CurrentContext)
        {
            Name = $"{Name}_description",
            CssClass = $"form-control jjlookup {GetFeedbackIcon(inputValue?.ToString(), description)} {CssClass}",
            InputType = InputType.Text, 
            MaxLength = MaxLength,
            Text = description,
            Attributes = Attributes.DeepCopy(),
            ToolTip = ToolTip,
            Enabled = false
        };
        
        descriptionTextBox.Attributes["style"] = "flex:10";
        
        div.AppendComponent(idTextBox);
        div.AppendComponent(descriptionTextBox);
        div.AppendComponent(new JJLinkButton
        {
            Name = $"btn_{Name}",
            Enabled = Enabled,
            ShowAsButton = true,
            IconClass = "fa fa-search"
        });
        return div;
    }

    private string? GetFeedbackIcon(string? value, string? description)
    {
        if (!string.IsNullOrEmpty(value) & !string.IsNullOrEmpty(description))
            return " jj-icon-success ";
        if (!string.IsNullOrEmpty(value) & string.IsNullOrEmpty(description))
            return " jj-icon-warning";
        return null;
    }


   

    /// <summary>
    /// Recovers the description based on the selected value
    /// </summary>
    /// <returns>Returns the description of the id</returns>
    public async Task<string> GetDescriptionAsync()
    {
        return await LookupService.GetDescriptionAsync(ElementMap, FormStateData, SelectedValue, OnlyNumbers);
    }

    private bool IsAjaxGetDescription()
    {
        string lookupAction = CurrentContext.Request.QueryString("lookupAction");
        return "getDescription".Equals(lookupAction);
    }


    private bool IsLookupRoute()
    {
        string panelName = string.Empty;
        if (Attributes.TryGetValue("panelName", out var attribute))
            panelName = attribute;

        string lookupRoute = CurrentContext.Request.QueryString("lookup-" + panelName);
        return Name.Equals(lookupRoute);
    }

    //todo: mover para LookupFactory
    public static async Task<ComponentResult?> GetResultFromPanel(JJDataPanel view)
    {
        string lookupRoute = view.CurrentContext.Request.QueryString("lookup-" + view.Name);
        if (string.IsNullOrEmpty(lookupRoute))
            return null;

        var field = view.FormElement.Fields.ToList().Find(x => x.Name.Equals(lookupRoute));
        if (field == null)
            return null;
        
        var formStateData = new FormStateData(view.Values, view.UserValues, view.PageState);
        var lookup = await view.ComponentFactory.Controls
            .CreateAsync(view.FormElement, field, formStateData, view.FieldNamePrefix, view.Name) as JJLookup;
        return await lookup!.GetResultAsync();
    }
}