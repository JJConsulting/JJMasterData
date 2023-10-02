#nullable disable

using System.Runtime.Serialization;
using JJMasterData.Core.DataDictionary.Models;
using Newtonsoft.Json;

namespace JJMasterData.ConsoleApp.Models.FormElementMigration;

[DataContract]
public class MetadataApiOptions
{
    /// <summary>
    /// Get all records. Verb GET
    /// </summary>
    [JsonProperty("enableGetAll")]
    public bool EnableGetAll { get; set; }

    /// <summary>
    /// Get a record detail. Verb GET
    /// </summary>
    [JsonProperty("enableGetDetail")]
    public bool EnableGetDetail { get; set; }

    /// <summary>
    /// Add new records. Verb POST
    /// </summary>
    [JsonProperty("enableAdd")]
    public bool EnableAdd { get; set; }

    /// <summary>
    /// Update records. Verb PUT
    /// </summary>
    [JsonProperty("enableUpdate")]
    public bool EnableUpdate { get; set; }

    /// <summary>
    /// Update some especifics fields. Verb PATCH
    /// </summary>
    [JsonProperty("enableUpdatePart")]
    public bool EnableUpdatePart { get; set; }

    /// <summary>
    /// Delete a record. Verb DEL
    /// </summary>
    [JsonProperty("enableDel")]
    public bool EnableDel { get; set; }

    /// <summary>
    /// Json Format
    /// </summary>
    [JsonProperty("formatType")]
    public ApiJsonFormat FormatType { get; set; }

    /// <summary>
    /// Aways apply UserId (from login) as filter or on set
    /// </summary>
    [JsonProperty("applyUserIdOn")]
    public string ApplyUserIdOn { get; set; }


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