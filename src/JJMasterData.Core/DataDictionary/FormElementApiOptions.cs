#nullable enable

namespace JJMasterData.Core.DataDictionary;

using System.Runtime.Serialization;

[DataContract]
public class FormElementApiOptions
{
    /// <summary>
    /// Get all records. Verb GET
    /// </summary>
    [DataMember]
    public bool EnableGetAll { get; set; }

    /// <summary>
    /// Get a record detail. Verb GET
    /// </summary>
    [DataMember]
    public bool EnableGetDetail { get; set; }

    /// <summary>
    /// Add new records. Verb POST
    /// </summary>
    [DataMember]
    public bool EnableAdd { get; set; }

    /// <summary>
    /// Update records. Verb PUT
    /// </summary>
    [DataMember]
    public bool EnableUpdate { get; set; }

    /// <summary>
    /// Update some especifics fields. Verb PATCH
    /// </summary>
    [DataMember]
    public bool EnableUpdatePart { get; set; }

    /// <summary>
    /// Delete a record. Verb DEL
    /// </summary>
    [DataMember]
    public bool EnableDel { get; set; }

    /// <summary>
    /// Json Format
    /// </summary>
    [DataMember]
    public ApiJsonFormat FormatType { get; set; }

    /// <summary>
    /// Always apply UserId (from login) as filter or on set
    /// </summary>
    [DataMember]
    public string? ApplyUserIdOn { get; set; }


    public FormElementApiOptions()
    {
        FormatType = ApiJsonFormat.Lowercase;
    }

    /// <summary>
    /// Format the field according to the dictionary parameterization
    /// </summary>
    public string GetFieldNameParsed(string fieldName)
    {
        return FormatType == ApiJsonFormat.Lowercase ? fieldName.ToLower() : fieldName;
    }


    public bool HasSetMethod()
    {
        return EnableAdd ||
               EnableUpdate ||
               EnableUpdatePart ||
               EnableDel;
    }
}