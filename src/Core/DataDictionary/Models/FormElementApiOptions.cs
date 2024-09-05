#nullable enable

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementApiOptions
{
    /// <summary>
    /// Get all records. GET Verb.
    /// </summary>
    [JsonProperty("enableGetAll")]
    public bool EnableGetAll { get; set; }

    /// <summary>
    /// Get a record detail. GET Verb.
    /// </summary>
    [JsonProperty("enableGetDetail")]
    public bool EnableGetDetail { get; set; }

    /// <summary>
    /// Add new records. POST Verb.
    /// </summary>
    [JsonProperty("enableAdd")]
    public bool EnableAdd { get; set; }

    /// <summary>
    /// Update records. PUT Verb.
    /// </summary>
    [JsonProperty("enableUpdate")]
    public bool EnableUpdate { get; set; }

    /// <summary>
    /// Update some specifics fields. PATCH Verb ,
    /// </summary>
    [JsonProperty("enableUpdatePart")]
    public bool EnableUpdatePart { get; set; }

    /// <summary>
    /// Delete a record. DEL Verb.
    /// </summary>
    [JsonProperty("enableDel")]
    public bool EnableDel { get; set; }
    
    [JsonProperty("formatType")]
    [Display(Name = "Json Formatting")]
    public ApiJsonFormatting JsonFormatting { get; set; } = ApiJsonFormatting.Lowercase;

    /// <summary>
    /// Always apply UserId (from login) as filter or on set
    /// </summary>
    [JsonProperty("applyUserIdOn")]
    [Display(Name = "Apply User Id On")]
    public string? ApplyUserIdOn { get; set; }


    /// <returns>
    /// The field name according to the <see cref="JsonFormatting"/> property. 
    /// </returns>
    public string GetJsonFieldName(string fieldName)
    {
        return JsonFormatting == ApiJsonFormatting.Lowercase ? fieldName.ToLower() : fieldName;
    }
    
    public bool HasSetMethod()
    {
        return EnableAdd ||
               EnableUpdate ||
               EnableUpdatePart ||
               EnableDel;
    }

    public FormElementApiOptions DeepCopy()
    {
        return (FormElementApiOptions)MemberwiseClone();
    }
}