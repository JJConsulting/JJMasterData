#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class SqlCommandAction : BasicAction, ISubmittableAction
{
    [JsonPropertyName("isSubmit")]
    [Display(Name = "Is Submit")]
    public bool IsSubmit { get; set; }

    /// <summary>
    /// Comando SQL a ser executado, aceita expression
    /// </summary>
    [JsonPropertyName("commandSQL")]
    public string SqlCommand { get; set; } = null!;

    /// <summary>
    /// Aplicar somenter as linhas selecionadas (default=false)
    /// </summary>
    /// <remarks>
    /// Valido somente para contexto da toolbar, 
    /// onde o comando é aplicado para cada linha selecionada
    /// </remarks>
    [JsonPropertyName("applyOnSelected")]
    [Display(Name = "Apply only on selected lines")]
    public bool ApplyOnSelected { get; set; }

    /// <summary>
    /// Redirects to this URL after the command is executed successfully.
    /// </summary>
    [JsonPropertyName("redirectUrl")]
    [Display(Name = "Redirect Url")]
    public string? RedirectUrl { get; set; }
    
    public SqlCommandAction()
    {
        Icon = IconType.Play;
    }

    public override bool IsCustomAction => true;
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}