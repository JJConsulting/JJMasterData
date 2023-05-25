#nullable enable

using System;
using JJMasterData.Commons.Data.Entity;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Field-specific information in the form, inherits from ElementField
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>

public class FormElementField : ElementField
{
    public const string PlaceholderAttribute = "placeholder";
    public const string RowsAttribute = "rows";
    public const string PopUpSizeAttribute = "popupsize";
    public const string PopUpTitleAttribute = "popuptitle";
    public const string AutocompletePickerAttribute = "autocompletePicker";
    public const string MinValueAttribute = "minValue";
    public const string MaxValueAttribute = "maxValue";

    private FormElementFieldActions? _actions;

    [JsonProperty("component")] public FormComponent Component { get; set; }

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonProperty("visibleExpression")]
    public string? VisibleExpression { get; set; }

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonProperty("enableExpression")]
    public string? EnableExpression { get; set; }


    /// <summary>
    /// Field position in a line
    /// </summary>
    [JsonProperty("order")]
    public int Order { get; set; }

    /// <summary>
    /// Line counter, used to break the line in the form (row class)
    /// </summary>
    /// <remarks>
    /// Used to manually specify the size of fields on the form
    /// Example:
    /// <code>
    ///     //This field is in line 1 
    ///     FormElementField f1 = FormElement.Fields["fieldname1"];
    ///     f1.LineGroup = 1;
    ///     f1.CssClass = "col-sm-12";
    ///     
    ///     //This field is in line 2
    ///     FormElementField f2 = FormElement.Fields["fieldname2"];
    ///     f2.LineGroup = 2;
    ///     f2.CssClass = "col-sm-6";
    /// 
    ///     //This field is in line 2
    ///     FormElementField f3 = FormElement.Fields["fieldname3"];
    ///     f3.LineGroup = 2;
    ///     f3.CssClass = "col-sm-6";
    /// </code>
    /// </remarks>
    [JsonProperty("lineGroup")]
    public int LineGroup { get; set; }

    /// <summary>
    /// Class name (CSS) to be appended in object group rendering
    /// </summary>
    [JsonProperty("cssClass")]
    public string? CssClass { get; set; }

    /// <summary>
    /// Help text will be displayed next to the label
    /// </summary>
    [JsonProperty("helpDescription")]
    public string? HelpDescription { get; set; }

    /// <summary>
    /// Relationship specific settings
    /// </summary>
    [JsonProperty("dataItem")]
    public FormElementDataItem? DataItem { get; set; }

    /// <summary>
    /// File-specific settings
    /// </summary>
    [JsonProperty("dataFile")]
    public FormElementDataFile? DataFile { get; set; }

    /// <summary>
    /// Collection of arbitrary (rendering-only) attributes that do not match control properties
    /// </summary>
    [JsonProperty("attributes")]
    public IDictionary<string, dynamic> Attributes { get; set; } =
        new Dictionary<string, dynamic>(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Allows exporting the field (Default=true)
    /// </summary>
    [JsonProperty("export")]
    public bool Export { get; set; }

    /// <summary>
    /// Validates possibly dangerous values in the request for .NET Framework
    /// </summary>
    /// <remarks>
    /// Important for lower versions of .NET Framework to enable the parameter: 
    /// httpRuntime requestValidationMode="4.5" ... 
    /// </remarks>
    [JsonProperty("validateRequest")]
    public bool ValidateRequest { get; set; }

    /// <summary>
    /// Ao alterar o conteúdo recarrega todos os campos do formulário
    /// (Default=false)
    /// </summary>
    /// <remarks>
    /// Normalmente utilizado para atualizar componente combobox ou searchbox que utilizam 
    /// um valor do formulário como referência na query.
    /// <para/>Exemplo:
    /// "SELECT ID, DESCR FROM TB_FOO WHERE TPVEND = {campo_tpvend}"
    /// </remarks>
    [JsonProperty("autoPostBack")]
    public bool AutoPostBack { get; set; }

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonProperty("triggerExpression")]
    public string? TriggerExpression { get; set; }

    /// <summary>
    /// Number of decimal places. The default value is 0.
    /// </summary>
    /// <remarks>
    /// Property valid only for numeric types
    /// </remarks>
    [JsonProperty("numberOfDecimalPlaces")]
    public int NumberOfDecimalPlaces { get; set; }

    /// <summary>
    /// Field grouper panel id
    /// </summary>
    /// <remarks>
    /// Id references a FormElementPanel
    /// </remarks>
    [JsonProperty("panelId")]
    public int PanelId { get; set; }

    [JsonProperty("actions")]
    public FormElementFieldActions Actions
    {
        get => _actions ??= new FormElementFieldActions();
        set => _actions = value;
    }

    /// <summary>
    /// Internal developer notes
    /// </summary>
    [JsonProperty("internalNotes")]
    public string? InternalNotes { get; set; }

    public FormElementField()
    {
        Component = FormComponent.Text;
        DataItem = new FormElementDataItem();
        Export = true;
        ValidateRequest = true;
        VisibleExpression = "val:1";
        EnableExpression = "val:1"; 
    }

    public FormElementField(ElementField elementField)
    {
        Name = elementField.Name;
        Label = elementField.Label;
        DataType = elementField.DataType;
        Size = elementField.Size;
        DefaultValue = elementField.DefaultValue;
        IsRequired = elementField.IsRequired;
        IsPk = elementField.IsPk;
        AutoNum = elementField.AutoNum;
        Filter = elementField.Filter;
        DataBehavior = elementField.DataBehavior;

        switch (elementField.DataType)
        {
            case FieldType.Date:
            case FieldType.DateTime:
            case FieldType.DateTime2:
                Component = FormComponent.Date;
                break;
            case FieldType.Int:
                Component = FormComponent.Number;
                break;
            default:
            {
                Component = elementField.Size > 290 ? FormComponent.TextArea : FormComponent.Text;
                break;
            }
        }

        VisibleExpression = "val:1";
        EnableExpression = "val:1";
        if (elementField.IsPk)
        {
            if (elementField.AutoNum)
            {
                EnableExpression = "exp:{pagestate} = 'FILTER'";
                VisibleExpression = "exp:{pagestate} <> 'INSERT'";
            }
            else
            {
                EnableExpression = "exp:{pagestate} <> 'UPDATE'";
            }
        }

        DataItem = new FormElementDataItem();
        Export = true;
        ValidateRequest = true;
    }


    public string? GetAttr(string key)
    {
        if (Attributes.TryGetValue(key, out var attribute))
            return attribute.ToString();
        return string.Empty;
    }

    public void SetAttr(string key, object value)
    {
        Attributes[key] = value;

        if (string.IsNullOrEmpty(value.ToString()))
            Attributes?.Remove(key);
    }
}