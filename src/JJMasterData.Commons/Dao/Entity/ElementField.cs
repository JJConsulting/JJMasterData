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
    /// <example>
    /// <para/> Example using [val:] + text
    /// <para/> Example1: val:a simple text;
    /// <para/> Example2: val:10000;
    /// <code lang="c#">
    /// var field = new ElementField();
    /// field.DefaultValue = "val:test";
    /// </code>
    /// <para/> Example using [exp:] + expression
    /// <para/> Example1: exp:{field1};
    /// <para/> Example2: exp:({field1} + 10) * {field2};
    /// <code lang="c#">
    /// var field = new ElementField();
    /// field.DefaultValue = "exp:{UserId}";
    /// </code>
    /// <para/> Example using [sql:] + query
    /// <para/> Example1: sql:select 'foo';
    /// <para/> Example2: sql:select count(*) from table1;
    /// <code lang="c#">
    /// var field = new ElementField();
    /// field.DefaultValue = "sql:select field2 from table1 where field1 = '{field1}'";
    /// </code>
    /// <para/> Example using [protheus:] + "UrlProtheus", "NameFunction", "Parameters"
    /// <para/> Example1: protheus:"http://localhost/jjmain.apw","u_test","";
    /// <para/> Example2: protheus:"http://localhost/jjmain.apw","u_test","{field1};parm2";
    /// <code lang="c#">
    /// var field = new ElementField();
    /// field.DefaultValue = "protheus:'http://10.0.0.6:8181/websales/jjmain.apw', 'u_vldpan', '1;2'";
    /// </code>
    /// *Warnings: For Protheus calls apply JJxFun patch and configure http connection in Protheus
    /// </example>
    /// <remarks>
    /// <para/> How to do an expression to hide or show an object:
    /// <para/>Form Fields, UserValues ou Session = exp:{FIELD_NAME}
    /// <para/>*Warnings: 
    /// The contents between {} (brace) will be replaced with current values ​​at runtime..
    /// Order:
    /// <para>1) UserValues (object property)</para>
    /// <para>2) Form Fields (Field Name)</para>
    /// <para>3) Keywords (pagestate)</para>
    /// <para>4) User Session</para>
    /// <para/>Examples of keywords:
    /// <para/>{pagestate} = Page Status: {pagestate} = "INSERT" | "UPDATE" | "VIEW" | "LIST" | "FILTER" | "IMPORT"
    /// <para/>{objname} = Name of the field that triggered the autopostback event
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