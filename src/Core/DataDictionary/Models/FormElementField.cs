#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Serialization;
using JJMasterData.Core.DataDictionary.Models.Actions;


namespace JJMasterData.Core.DataDictionary.Models;

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
    public const string MinValueAttribute = "min";
    public const string MaxValueAttribute = "max";
    public const string StepAttribute = "step";
    public const string IsSwitchAttribute = "is-switch";
    public const string IsButtonAttribute = "is-button";
    public const string CultureInfoAttribute = "culture-info";

    [JsonPropertyName("component")]
    [Display(Name = "Component")]
    public FormComponent Component { get; set; }

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonPropertyName("visibleExpression")]
    [Display(Name = "Visible Expression")]
    [SyncExpression]
    [Required]
    public string VisibleExpression { get; set; }

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonPropertyName("enableExpression")]
    [Display(Name = "Enable Expression")]
    [SyncExpression]
    [Required]
    public string EnableExpression { get; set; }

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
    [JsonPropertyName("lineGroup")]
    [Display(Name = "Line Group")]
    public int LineGroup { get; set; }

    /// <summary>
    /// Class name (CSS) to be appended in object group rendering
    /// </summary>
    [JsonPropertyName("cssClass")]
    [Display(Name = "CSS Class")]
    public string? CssClass { get; set; }

    /// <summary>
    /// Help text will be displayed next to the label
    /// </summary>
    [JsonPropertyName("helpDescription")]
    [Display(Name = "Tooltip")]
    public string? HelpDescription { get; set; }

    /// <summary>
    /// Relationship specific settings
    /// </summary>
    [JsonPropertyName("dataItem")]
    public FormElementDataItem? DataItem { get; set; }

    /// <summary>
    /// File-specific settings
    /// </summary>
    [JsonPropertyName("dataFile")]
    public FormElementDataFile? DataFile { get; set; }

    /// <summary>
    /// Collection of arbitrary (rendering-only) attributes that do not match control properties
    /// </summary>
    [JsonPropertyName("attributes")]
    [JsonConverter(typeof(DictionaryStringObjectJsonConverter))]
    public Dictionary<string, object?> Attributes { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Allows exporting the field (Default=true)
    /// </summary>
    [JsonPropertyName("export")]
    [Display(Name = "Enable Exportation")]
    public bool Export { get; set; }

    /// <summary>
    /// Validates possibly dangerous values in the request for .NET Framework
    /// </summary>
    /// <remarks>
    /// Important for lower versions of .NET Framework to enable the parameter: 
    /// httpRuntime requestValidationMode="4.5" ... 
    /// </remarks>
    [JsonPropertyName("validateRequest")]
    [Display(Name = "Validate Request")]
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
    [JsonPropertyName("autoPostBack")]
    [Display(Name = "Auto Reload")]
    public bool AutoPostBack { get; set; }


    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonPropertyName("triggerExpression")]
    [Display(Name = "Trigger Expression")]
    [AsyncExpression]
    public string? TriggerExpression { get; set; }

    /// <summary>
    /// Number of decimal places. The default value is 0.
    /// </summary>
    /// <remarks>
    /// Property valid only for numeric types
    /// </remarks>
    [JsonPropertyName("numberOfDecimalPlaces")]
    [Display(Name = "Number of Decimal Places")]
    public int NumberOfDecimalPlaces { get; set; }

    /// <summary>
    /// This id references a FormElementPanel
    /// </summary>
    [JsonPropertyName("panelId")]
    public int PanelId { get; set; }

    [JsonPropertyName("actions")]
    [JsonConverter(typeof(FormElementFieldActionListConverter))]
    public FormElementFieldActionList Actions { get; set; }

    /// <summary>
    /// Internal developer notes
    /// </summary>
    [JsonPropertyName("internalNotes")]
    public string? InternalNotes { get; set; }

    [JsonPropertyName("gridAlignment")]
    [Display(Name = "Alignment At Grid")]
    public GridAlignment GridAlignment { get; set; }
    
    /// <summary>
    /// Template used to render the field at the Grid.
    /// </summary>
    [LanguageInjection("html")]
    [JsonPropertyName("gridRenderingTemplate")]
    [Display(Name = "Rendering Template")]
    public string? GridRenderingTemplate { get; set; }
    
    [JsonPropertyName("encodeHtml")]
    [Display(Name = "Encode HTML")]
    public bool EncodeHtml { get; set; } = true;

    /// <summary>
    /// The field will be disabled but the value send to the server.
    /// </summary>
    [JsonIgnore]
    public string? ReadOnlyExpression { get; set; }
    
    public FormElementField()
    {
        Component = FormComponent.Text;
        Export = true;
        ValidateRequest = true;
        VisibleExpression = "val:1";
        EnableExpression = "val:1";
        Actions = new();
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
                EnableExpression = "val:{IsFilter}";
                VisibleExpression = "exp:'{PageState}' <> 'Insert'";
            }
            else
            {
                EnableExpression = "exp:'{PageState}' <> 'Update'";
            }
        }

        Export = true;
        ValidateRequest = true;
        Actions = new();
    }


    public object? GetAttr(string key)
    {
        if (Attributes.TryGetValue(key, out var attribute))
            return attribute;
        return string.Empty;
    }

    public void SetAttr(string key, object value)
    {
        Attributes[key] = value;

        if (string.IsNullOrEmpty(value?.ToString()))
            Attributes?.Remove(key);
    }

    /// <summary>
    /// Set field visibility
    /// </summary>
    public void SetVisible(bool value)
    {
        VisibleExpression = value ? "val:1" : "val:0";
    }

    /// <summary>
    /// Set if the field is enabled.
    /// </summary>
    /// <param name="value"></param>
    public void SetEnabled(bool value)
    {
        EnableExpression = value ? "val:1" : "val:0";
    }
    
    public bool SupportsFloatingLabel() =>
        Component
            is FormComponent.Text
            or FormComponent.TextArea
            or FormComponent.Hour
            or FormComponent.Date
            or FormComponent.DateTime
            or FormComponent.Number
            or FormComponent.Cep
            or FormComponent.Cpf
            or FormComponent.CnpjCpf
            or FormComponent.Tel
            or FormComponent.ComboBox
        && DataItem?.SupportsFloatingLabels() != false;

    /// <summary>
    /// Set if the field is enabled.
    /// </summary>
    /// <param name="value"></param>
    public void SetReadOnly(bool value)
    {
        ReadOnlyExpression = value ? "val:1" : "val:0";
    }

    public new FormElementField DeepCopy()
    {
        var copiedField = (FormElementField)MemberwiseClone();

        copiedField.DataItem = DataItem?.DeepCopy();
        copiedField.DataFile = DataFile?.DeepCopy();
        copiedField.Attributes = new Dictionary<string, object?>(Attributes);
        copiedField.Actions = Actions.DeepCopy();
        copiedField.Filter = Filter.DeepCopy();

        return copiedField;
    }
}