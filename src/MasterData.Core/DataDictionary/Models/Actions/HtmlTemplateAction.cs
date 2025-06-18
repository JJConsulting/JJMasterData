using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class HtmlTemplateAction : BasicAction
{
    [JsonPropertyName("sqlCommand")]
    [Display(Name = "SQL Command")]
    public string SqlCommand { get; set; }

    [JsonPropertyName("htmlTemplate")]
    [Display(Name = "HTML Template")]
    public string HtmlTemplate { get; set; }

    public HtmlTemplateAction()
    {
        Name = "html-template";
        Icon = IconType.RegularFileLines;
        Text = "Template";
        ShowAsButton = false;
    }

    
    [JsonIgnore]
    public override bool IsUserDefined => true;
    
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}