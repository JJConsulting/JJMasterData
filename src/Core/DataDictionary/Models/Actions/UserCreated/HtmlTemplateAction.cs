using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class HtmlTemplateAction : UserCreatedAction
{
    [JsonProperty("sqlCommand")]
    [Display(Name = "SQL Command")]
    public string SqlCommand { get; set; }

    [JsonProperty("htmlTemplate")]
    [Display(Name = "HTML Template")]
    public string HtmlTemplate { get; set; }

    public HtmlTemplateAction()
    {
        Name = "html-template";
        Icon = IconType.RegularFileLines;
        Text = "Template";
        ShowAsButton = true;
    }
    
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}