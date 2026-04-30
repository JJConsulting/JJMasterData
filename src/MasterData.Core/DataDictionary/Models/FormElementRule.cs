using JJMasterData.Commons.Data.Entity.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementRule
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Rule Type")]
    [JsonPropertyName("language")]
    public RuleLanguage Language { get; set; } = RuleLanguage.Sql;

    [Display(Name = "Script")]
    [JsonPropertyName("script")]
    public string Script { get; set; } = string.Empty;

    public FormElementRule DeepCopy()
    {
        return (FormElementRule)MemberwiseClone();
    }
}
