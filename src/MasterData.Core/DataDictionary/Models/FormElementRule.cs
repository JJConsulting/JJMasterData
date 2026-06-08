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

    [Display(Name = "Run On Before Insert")]
    [JsonPropertyName("runOnBeforeInsert")]
    public bool RunOnBeforeInsert { get; set; } = true;

    [Display(Name = "Run On Before Update")]
    [JsonPropertyName("runOnBeforeUpdate")]
    public bool RunOnBeforeUpdate { get; set; } = true;

    [Display(Name = "Run On Before Import")]
    [JsonPropertyName("runOnBeforeImport")]
    public bool RunOnBeforeImport { get; set; } = true;
    
    [Display(Name = "Run On Before Delete")]
    [JsonPropertyName("runOnBeforeDelete")]
    public bool RunOnBeforeDelete { get; set; }

    [Display(Name = "Rule Type")]
    [JsonPropertyName("language")]
    public RuleLanguage Language { get; set; } = RuleLanguage.Sql;

    [Display(Name = "Script")]
    [JsonPropertyName("script")]
    public string Script { get; set; } = string.Empty;

    public bool ShouldRun(PageState pageState)
    {
        return pageState switch
        {
            PageState.Insert => RunOnBeforeInsert,
            PageState.Update => RunOnBeforeUpdate,
            PageState.Import => RunOnBeforeImport,
            PageState.Delete => RunOnBeforeDelete,
            _ => false
        };
    }

    public FormElementRule DeepCopy()
    {
        return (FormElementRule)MemberwiseClone();
    }
}
