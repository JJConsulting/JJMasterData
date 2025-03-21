﻿#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Representation of a metadata field.
/// </summary>
/// <remarks>2017-03-22 - JJTeam</remarks>
[DebuggerDisplay("Name = {Name}, DataType = {DataType}")]
public class ElementField
{
    /// <summary>
    /// Internal field id
    /// </summary>
    [JsonPropertyName("fieldid")]
    public int FieldId { get; set; }

    /// <summary>
    /// Column name
    /// </summary>
    /// <remarks>
    /// When in JJGridView, the "::ASC" OR "::DESC" tags can be used
    /// in the column name to indicate the data order.
    /// </remarks>
    [JsonPropertyName("fieldname")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description on the form
    /// </summary>
    [JsonPropertyName("label")]
    [Display(Name = "Label")]
    public string? Label { get; set; }

    [JsonIgnore]
    public string LabelOrName => string.IsNullOrEmpty(Label) ? Name : Label!;
    
    /// <summary>
    /// Data Type
    /// Default NVARCHAR
    /// </summary>
    [JsonPropertyName("datatype")]
    [Display(Name = "Data Type")]
    public FieldType DataType { get; set; } = FieldType.Varchar;

    /// <summary>
    /// Filter Parameters
    /// </summary>
    [JsonPropertyName("filter")]
    public ElementFilter Filter { get; set; } = new();

    /// <summary>
    /// Filed Size
    /// </summary>
    [JsonPropertyName("size")]
    [Display(Name = "Size")]
    public int Size { get; set; }

    [JsonPropertyName("numberOfDecimalPlaces")]
    [Display(Name = "Number of Decimal Places")]
    public int NumberOfDecimalPlaces { get; set; }
    
    /// <summary>
    /// Default field initializer
    /// <para/> Expression for a default value
    /// <para/> Tipo [val:] returns a value;
    /// <para/> Tipo [exp:] returns the result of the expression;
    /// <para/> Tipo [sql:] returns the result of a sql command;
    /// <para/> Tipo [protheus:] returns the result of a Protheus function;
    /// </summary>
    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [JsonPropertyName("defaultvalue")]
    [Display(Name = "Default Value Expression")]
    public string? DefaultValue { get; set; }
    
    /// <summary>
    /// Required field (Default=false)
    /// </summary>
    [JsonPropertyName("isrequired")]
    [Display(Name="Required")]
    public bool IsRequired { get; set; }

    /// <summary>
    /// Primary Key (Default=false)
    /// </summary>
    [JsonPropertyName("ispk")]
    [Display(Name="Primary Key")]
    public bool IsPk { get; set; }

    /// <summary>
    /// Auto Numerical (Identity) (Default=false)
    /// </summary>
    [JsonPropertyName("autonum")]
    [Display(Name = "Identity")]
    public bool AutoNum { get; set; }

    /// <summary>
    /// Field behavior in relation to the database
    /// Default value: Real
    /// </summary>
    /// <remarks>
    /// <para/>Real = Used in both Get and Set operations
    /// <para/>ViewOnly = Used to only in theGet operation
    /// <para/>WriteOnly = Used to only in the Set operation
    /// <para/>Virtual = Ignored in database operations
    /// </remarks>
    [JsonPropertyName("databehavior")]
    [Display(Name = "Behavior")]
    public FieldBehavior DataBehavior { get; set; } = FieldBehavior.Real;

    /// <summary>
    /// Apply this field on delete filter on procedure
    /// </summary>
    [JsonIgnore]
    public bool EnableOnDelete { get; set; } = true;

    public ElementField DeepCopy()
    {
        var copy = (ElementField)MemberwiseClone();
        copy.Filter = Filter.DeepCopy();
        return copy;
    }
}