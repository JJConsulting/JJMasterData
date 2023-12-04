#nullable enable

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Field Info
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
public class ElementField
{
    /// <summary>
    /// Internal field id
    /// </summary>
    [JsonProperty("fieldid")]
    public int FieldId { get; set; }

    /// <summary>
    /// Column name
    /// </summary>
    /// <remarks>
    /// When in JJGridView, the "::ASC" OR "::DESC" tags can be used
    /// in the column name to indicate the data order.
    /// </remarks>
    [JsonProperty("fieldname")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description on the form
    /// </summary>
    [JsonProperty("label")]
    [Display(Name = "Label")]
    public string? Label { get; set; }

    [JsonIgnore]
    public string LabelOrName => string.IsNullOrEmpty(Label) ? Name : Label!;
    
    /// <summary>
    /// Data Type
    /// Default NVARCHAR
    /// </summary>
    [JsonProperty("datatype")]
    [Display(Name = "Data Type")]
    public FieldType DataType { get; set; } = FieldType.Varchar;

    /// <summary>
    /// Filter Parameters
    /// </summary>
    [JsonProperty("filter")]
    public ElementFilter Filter { get; set; } = new();

    /// <summary>
    /// Filed Size
    /// </summary>
    [JsonProperty("size")]
    [Display(Name = "Size")]
    public int Size { get; set; }

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
    [JsonProperty("defaultvalue")]
    [AsyncExpression]
    [Display(Name = "Default Value Expression")]
    public string? DefaultValue { get; set; }
    
    /// <summary>
    /// Required field (Default=false)
    /// </summary>
    [JsonProperty("isrequired")]
    public bool IsRequired { get; set; }

    /// <summary>
    /// Primary Key (Default=false)
    /// </summary>
    [JsonProperty("ispk")]
    public bool IsPk { get; set; }

    /// <summary>
    /// Auto Numerical (Identity) (Default=false)
    /// </summary>
    [JsonProperty("autonum")]
    [Display(Name = "Identity")]
    public bool AutoNum { get; set; }

    /// <summary>
    /// Field behavior in relation to the database
    /// Default (Real)
    /// </summary>
    /// <remarks>
    /// <para/>Real     = Used to Get and Set operations
    /// <para/>VIEWONLY = Used to only Get operation
    /// <para/>VIRTUAL  = Ignored in database operations
    /// </remarks>
    [JsonProperty("databehavior")]
    [Display(Name = "Data Behavior")]
    public FieldBehavior DataBehavior { get; set; } = FieldBehavior.Real;

    /// <summary>
    /// Apply this field on delete filter on procedure
    /// </summary>
    [JsonIgnore]
    public bool EnableOnDelete { get; set; } = true;
}