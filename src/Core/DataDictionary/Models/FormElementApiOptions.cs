#nullable enable

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementApiOptions
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
    [Display(Name = "Json Formatting")]
    public ApiJsonFormatting JsonFormatting { get; set; } = ApiJsonFormatting.Lowercase;

    /// <summary>
    /// Always apply UserId (from login) as filter or on set
    /// </summary>
    [JsonProperty("applyUserIdOn")]
    [Display(Name = "Apply User Id On")]
    public string? ApplyUserIdOn { get; set; }


    /// <summary>
    /// Format the field according to the dictionary parameterization
    /// </summary>
    public string GetFieldNameParsed(string fieldName)
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