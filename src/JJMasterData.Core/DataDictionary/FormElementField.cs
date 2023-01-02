using System;
using System.Collections;
using System.Runtime.Serialization;
using JJMasterData.Commons.Dao.Entity;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Informações específicas do campo no formulário, herda de ElementField
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class FormElementField : ElementField
{
    public const string PlaceholderAttribute = "placeholder";
    public const string RowsAttribute = "rows";
    public const string PopUpSizeAttribute = "popupsize";
    public const string PopUpTitleAttribute = "popuptitle";

    private FormElementFieldActions _action;
    
    
    [DataMember(Name = "component")] 
    public FormComponent Component { get; set; }

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [DataMember(Name = "visibleExpression")]
    public string VisibleExpression { get; set; }
    
    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [DataMember(Name = "enableExpression")]
    public string EnableExpression { get; set; }


    [DataMember(Name = "order")]
    public int Order { get; set; }
    

    /// <summary>
    /// Contador da linha, utilizado para quebrar a linha no form (classe row)
    /// </summary>
    /// <remarks>
    /// Utilizado para especificar manualmente o tamanho dos campos no formulário
    /// Exemplo:
    /// <code>
    ///     //Linha 1 com um campo
    ///     FormElementField f1 = FormElement.Fields["fieldname1"];
    ///     f1.LineGroup = 1;
    ///     f1.CssClass = "col-sm-12";
    ///     
    ///     //Linha 2 com dois campos
    ///     FormElementField f2 = FormElement.Fields["fieldname2"];
    ///     f2.LineGroup = 2;
    ///     f2.CssClass = "col-sm-6";
    ///     
    ///     FormElementField f3 = FormElement.Fields["fieldname3"];
    ///     f3.LineGroup = 2;
    ///     f3.CssClass = "col-sm-6";
    /// </code>
    /// </remarks>
    [DataMember(Name = "lineGroup")]
    public int LineGroup { get; set; }

    
    [DataMember(Name = "cssClass")]
    public string CssClass { get; set; }
    
    
    [DataMember(Name = "helpDescription")]
    public string HelpDescription { get; set; }

    /// <summary>
    /// Specific settings for data oriented controls
    /// </summary>
    [DataMember(Name = "dataItem")]
    public FormElementDataItem DataItem { get; set; }

    /// <summary>
    /// Specific settings for file oriented controls
    /// </summary>
    [DataMember(Name = "dataFile")]
    public FormElementDataFile DataFile { get; set; }

    /// <summary>
    /// Collection of arbitrary (rendering-only) attributes that do not match control properties
    /// </summary>
    [DataMember(Name = "attributes")]
    public Hashtable Attributes { get; set; }
    
    [DataMember(Name = "export")]
    public bool EnableExportation { get; set; }

    /// <summary>
    /// Validates possibly dangerous values in the request (Default=true)
    /// </summary>
    /// <remarks>
    /// Only works on .NET Framework 4.8
    /// Important for lower versions of .NET to enable the parameter:
    /// httpRuntime requestValidationMode="4.5" ... 
    /// </remarks>
    [DataMember(Name = "validateRequest")]
    public bool ValidateRequest { get; set; }

    /// <summary>
    /// When changing the content, it reloads all form fields
    /// (Default=false)
    /// </summary>
    /// <remarks>
    /// Usually used to update combobox or searchbox component that use dynamic values at DataItem.
    /// </remarks>
    [DataMember(Name = "autoPostBack")]
    public bool AutoPostBack { get; set; }

    /// <summary>
    /// Redo the expression whenever a field triggers AutoPostBack
    /// </summary>
    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [DataMember(Name = "triggerExpression")]
    public string TriggerExpression { get; set; }
    
    /// <remarks>
    /// Property valid only for numeric types
    /// </remarks>
    [DataMember(Name = "numberOfDecimalPlaces")]
    public int NumberOfDecimalPlaces { get; set; }
    
    [DataMember(Name = "panelId")]
    public int PanelId { get; set; }
    
    [DataMember(Name = "actions")]
    public FormElementFieldActions Actions
    {
        get => _action ??= new FormElementFieldActions();
        set => _action = value;
    }

    /// <summary>
    /// Internal developer note
    /// </summary>
    [DataMember(Name = "internalNotes")]
    public string InternalNotes { get; set; }
    
    /// <summary>
    /// Minimum value for number components
    /// </summary>
    [DataMember(Name = "minValue")]
    public float? MinValue { get; set; }
    
    /// <summary>
    /// Maximum value for number components
    /// </summary>
    [DataMember(Name = "maxValue")]
    public float? MaxValue { get; set; }


    public FormElementField()
    {
        Component = FormComponent.Text;
        DataItem = new FormElementDataItem();
        EnableExportation = true;
        ValidateRequest = true;
        VisibleExpression = "val:1";
        EnableExpression = "val:1";
        Attributes = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
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
        EnableExportation = true;
        ValidateRequest = true;
        Attributes = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
    }


    public string GetAttr(string key)
    {
        if (Attributes != null && Attributes.ContainsKey(key))
            return Attributes[key].ToString();
        return string.Empty;
    }

    public void SetAttr(string key, object value)
    {
        Attributes ??= new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        if (Attributes.ContainsKey(key))
            Attributes[key] = value;
        else
            Attributes.Add(key, value);

        if (value == null || string.IsNullOrEmpty(value.ToString()))
            Attributes.Remove(key);
    }
}