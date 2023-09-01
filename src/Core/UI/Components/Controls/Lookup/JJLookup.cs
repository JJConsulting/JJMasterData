#nullable enable
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
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

/// Represents a field with a value from another FormElement accessed via modal.
public class JJLookup : ControlBase
{
    private FormElement FormElement { get; set; }
    private ILookupService LookupService { get; }
    private IComponentFactory<JJTextBox> TextBoxFactory { get; }

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
            string val = GetAttr("modal-size");
            if (string.IsNullOrEmpty(val))
                return ModalSize.ExtraLarge;
            return (ModalSize)int.Parse(val);
        }
        set => SetAttr("modal-size", (int)value);
    }

    public string ModalTitle
    {
        get => GetAttr("modal-title");
        set => SetAttr("modal-title", value);
    }

    public new string? Text
    {
        get
        {
            if (AutoReloadFormFields && _text == null && Request.IsPost)
            {
                _text = Request[Name];
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
        IHttpRequest httpRequest,
        ILookupService lookupService,
        IEncryptionService encryptionService,
        IComponentFactory<JJTextBox> textBoxFactory) : base(httpRequest,encryptionService)
    {
        FormElement = formElement;
        ElementMap = field.DataItem?.ElementMap ?? throw new ArgumentException("ElementMap cannot be null.");
        FieldName = field.Name;
        LookupService = lookupService;
        TextBoxFactory = textBoxFactory;
        Enabled = true;
        AutoReloadFormFields = true;
        Name = field.Name;
        ModalSize = ModalSize.Large;
        ModalTitle = "Search";
        FormStateData = controlContext.FormStateData;
        UserValues = controlContext.FormStateData.UserValues;
        SelectedValue = controlContext.Value?.ToString();
        SetAttr(field.Attributes);

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
        return new RenderedComponentResult(await GetLookupHtml());
    }

    private async Task<HtmlBuilder> GetLookupHtml()
    {
        object? inputValue = SelectedValue;
        string? description = Text;

        if (string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(inputValue?.ToString()))
            description = await LookupService.GetDescriptionAsync(ElementMap, FormStateData, inputValue?.ToString(), OnlyNumbers);
        
        Attributes["lookup-field-name"] = FieldName;
        
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass("input-group mb-3 d-flex" );
        
        Attributes["lookup-description-url"] = LookupService.GetDescriptionUrl(FormElement.Name,FieldName,Name,FormStateData.PageState);

        var idTextBox = TextBoxFactory.Create();
        idTextBox.Name = Name;
        idTextBox.CssClass = $"form-control jj-lookup {GetFeedbackIcon(inputValue?.ToString(), description)} {CssClass}";
        idTextBox.InputType = OnlyNumbers ? InputType.Number : InputType.Text;
        idTextBox.MaxLength = MaxLength;
        idTextBox.Text = SelectedValue?.ToString();
        idTextBox.Attributes = Attributes.DeepCopy();
        idTextBox.ToolTip = ToolTip;
        idTextBox.ReadOnly = ReadOnly;
        idTextBox.Enabled = Enabled;


        await div.AppendControlAsync(idTextBox);
        
        if (!string.IsNullOrEmpty(FormElement.Fields[FieldName].DataItem!.ElementMap!.FieldDescription))
        {
            idTextBox.Attributes["style"] = "flex:2";
            
            var descriptionTextBox = TextBoxFactory.Create();
            descriptionTextBox.Name = $"{Name}-description";
            descriptionTextBox.CssClass = $"form-control jj-lookup {CssClass}";
            descriptionTextBox.InputType = InputType.Text;
            descriptionTextBox.MaxLength = MaxLength;
            descriptionTextBox.Text = description;
            descriptionTextBox.Attributes = Attributes.DeepCopy();
            descriptionTextBox.ToolTip = ToolTip;
            descriptionTextBox.Enabled = false;

            descriptionTextBox.Attributes["style"] = "flex:10";
            
            await div.AppendControlAsync(descriptionTextBox);
        }

        var formViewUrl = LookupService.GetFormViewUrl(ElementMap, FormStateData, Name);
        
        div.AppendComponent(new JJLinkButton
        {
            Name = $"btn_{Name}",
            Enabled = Enabled,
            ShowAsButton = true,
            OnClientClick = $$"""defaultModal.showUrl({ url: '{{formViewUrl}}' }, '{{ModalTitle}}', '{{(int)ModalSize}}')""",
            IconClass = "fa fa-search"
        });
        return div;
    }

    private static string? GetFeedbackIcon(string? value, string? description)
    {
        if (!string.IsNullOrEmpty(value) & !string.IsNullOrEmpty(description))
            return " jj-icon-success ";
        if (!string.IsNullOrEmpty(value) & string.IsNullOrEmpty(description))
            return " jj-icon-warning";
        return null;
    }
}