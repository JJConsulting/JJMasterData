using JJMasterData.Commons.Data.Entity.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementValidation
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Validation Type")]
    [JsonPropertyName("language")]
    public ValidationType Language { get; set; } = ValidationType.Sql;

    [Display(Name = "Script")]
    [JsonPropertyName("script")]
    public string Script { get; set; } = string.Empty;

    public FormElementValidation DeepCopy()
    {
        return (FormElementValidation)MemberwiseClone();
    }
}
