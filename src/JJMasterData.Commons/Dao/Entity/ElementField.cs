using System;
using System.Runtime.Serialization;
using JJMasterData.Commons.Language;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Dao.Entity;

/// <summary>
/// Informações do campo
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class ElementField
{
    /// <summary>
    /// Internal field id
    /// </summary>
    [DataMember(Name = "fieldid")]
    public int FieldId { get; set; }

    /// <summary>
    /// Column name
    /// </summary>
    /// <remarks>
    /// When in JJGridView, the "::ASC" OR "::DESC" tags can be used
    /// in the column name to indicate the data order.
    /// </remarks>
    [DataMember(Name = "fieldname")]
    public string Name { get; set; }

    /// <summary>
    /// Description on the form
    /// </summary>
    [DataMember(Name = "label")]
    public string Label { get; set; }

    /// <summary>
    /// Data Type
    /// Default NVARCHAR
    /// </summary>
    [DataMember(Name = "datatype")]
    public FieldType DataType { get; set; }

    /// <summary>
    /// Filter Parameters
    /// </summary>
    [DataMember(Name = "filter")]
    public ElementFilter Filter { get; set; }

    /// <summary>
    /// Filed Size
    /// </summary>
    [DataMember(Name = "size")]
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
    [DataMember(Name = "defaultvalue")]
    public string DefaultValue { get; set; }

    /// <summary>
    /// Required field (Default=false)
    /// </summary>
    [DataMember(Name = "isrequired")]
    public bool IsRequired { get; set; }

    /// <summary>
    /// Primary Key (Default=false)
    /// </summary>
    [DataMember(Name = "ispk")]
    public bool IsPk { get; set; }

    /// <summary>
    /// Auto Numerical (Identity) (Default=false)
    /// </summary>
    [DataMember(Name = "autonum")]
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
    [DataMember(Name = "databehavior")]
    public FieldBehavior DataBehavior { get; set; }
    
    /// <summary>
    /// For internal use of JJMasterData's table.
    /// </summary>
    [JsonIgnore]
    internal bool EnableOnDelete { get; set; } = true;

    public ElementField()
    {
        Filter = new ElementFilter();
        DataType = FieldType.NVarchar;
        DataBehavior = FieldBehavior.Real;
    }

    public string GetTranslatedLabel()
    {
        return string.IsNullOrEmpty(Label) ? Name : Translate.Key(Label);
    }
}