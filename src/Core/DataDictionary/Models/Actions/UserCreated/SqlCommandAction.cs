#nullable enable
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Azure.Core;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class SqlCommandAction : UserCreatedAction, ISubmittableAction
{
    [JsonProperty("isSubmit")]
    [Display(Name = "Is Submit")]
    public bool IsSubmit { get; set; }

    /// <summary>
    /// Comando SQL a ser executado, aceita expression
    /// </summary>
    [JsonProperty("commandSQL")]
    public string SqlCommand { get; set; } = null!;

    /// <summary>
    /// Aplicar somenter as linhas selecionadas (default=false)
    /// </summary>
    /// <remarks>
    /// Valido somente para contexto da toolbar, 
    /// onde o comando é aplicado para cada linha selecionada
    /// </remarks>
    [JsonProperty("applyOnSelected")]
    [Display(Name = "Apply only on selected lines")]
    public bool ApplyOnSelected { get; set; }

    /// <summary>
    /// Redirects to this URL after the command is executed successfully.
    /// </summary>
    [JsonProperty("redirectUrl")]
    [Display(Name = "Redirect Url")]
    public string? RedirectUrl { get; set; }
    
    public SqlCommandAction()
    {
        Icon = IconType.Play;
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}