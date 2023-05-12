#nullable disable

using System.Runtime.Serialization;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.FormElementUpdater.Models;

[DataContract]
public class MetadataApiOptions
{
    /// <summary>
    /// Get all records. Verb GET
    /// </summary>
    [DataMember(Name = "enableGetAll")]
    public bool EnableGetAll { get; set; }

    /// <summary>
    /// Get a record detail. Verb GET
    /// </summary>
    [DataMember(Name = "enableGetDetail")]
    public bool EnableGetDetail { get; set; }

    /// <summary>
    /// Add new records. Verb POST
    /// </summary>
    [DataMember(Name = "enableAdd")]
    public bool EnableAdd { get; set; }

    /// <summary>
    /// Update records. Verb PUT
    /// </summary>
    [DataMember(Name = "enableUpdate")]
    public bool EnableUpdate { get; set; }

    /// <summary>
    /// Update some especifics fields. Verb PATCH
    /// </summary>
    [DataMember(Name = "enableUpdatePart")]
    public bool EnableUpdatePart { get; set; }

    /// <summary>
    /// Delete a record. Verb DEL
    /// </summary>
    [DataMember(Name = "enableDel")]
    public bool EnableDel { get; set; }

    /// <summary>
    /// Json Format
    /// </summary>
    [DataMember(Name = "formatType")]
    public ApiJsonFormat FormatType { get; set; }

    /// <summary>
    /// Aways apply UserId (from login) as filter or on set
    /// </summary>
    [DataMember(Name = "applyUserIdOn")]
    public string? ApplyUserIdOn { get; set; }


    public MetadataApiOptions()
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


    public bool HasSetMehtod()
    {
        return EnableAdd ||
               EnableUpdate ||
               EnableUpdatePart ||
               EnableDel;
    }

}