using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class ScriptAction : BasicAction
{
    /// <summary>
    /// JS script that will be executed when the user clicks on the control.
    /// </summary>
    [JsonPropertyName("onClientClick")]
    [Display(Name = "Script")]
    [LanguageInjection("Javascript")]
    // ReSharper disable once InconsistentNaming
    public string OnClientClick { get; set; }
    
    [JsonIgnore]
    public override bool IsCustomAction => true;
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}