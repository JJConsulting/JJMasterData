#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementApiOptions
{
    /// <summary>
    /// Get all records. GET Verb.
    /// </summary>
    [JsonPropertyName("enableGetAll")]
    public bool EnableGetAll { get; set; }

    /// <summary>
    /// Get a record detail. GET Verb.
    /// </summary>
    [JsonPropertyName("enableGetDetail")]
    public bool EnableGetDetail { get; set; }

    /// <summary>
    /// Add new records. POST Verb.
    /// </summary>
    [JsonPropertyName("enableAdd")]
    public bool EnableAdd { get; set; }

    /// <summary>
    /// Update records. PUT Verb.
    /// </summary>
    [JsonPropertyName("enableUpdate")]
    public bool EnableUpdate { get; set; }

    /// <summary>
    /// Update some specifics fields. PATCH Verb ,
    /// </summary>
    [JsonPropertyName("enableUpdatePart")]
    public bool EnableUpdatePart { get; set; }

    /// <summary>
    /// Delete a record. DEL Verb.
    /// </summary>
    [JsonPropertyName("enableDel")]
    public bool EnableDel { get; set; }

    [JsonPropertyName("formatType")]
    [Display(Name = "Json Formatting")]
    public ApiJsonFormatting JsonFormatting { get; set; } = ApiJsonFormatting.Lowercase;

    /// <summary>
    /// Always apply UserId (from login) as filter or on set
    /// </summary>
    [JsonPropertyName("applyUserIdOn")]
    [Display(Name = "Apply User Id On")]
    public string? ApplyUserIdOn { get; set; }


    /// <returns>
    /// The field name according to the <see cref="JsonFormatting"/> property. 
    /// </returns>
    public string GetJsonFieldName(string fieldName)
    {
        return JsonFormatting == ApiJsonFormatting.Lowercase ? fieldName.ToLower() : fieldName;
    }
    
    public bool HasMethod() => HasGetMethod() || HasSetMethod();

    public bool HasGetMethod() => EnableGetDetail || EnableGetAll;

    public bool HasSetMethod() => EnableAdd || EnableUpdate || EnableUpdatePart || EnableDel;

    public FormElementApiOptions DeepCopy()
    {
        return (FormElementApiOptions)MemberwiseClone();
    }
}