using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class SqlCommandAction : UserCreatedAction, ISubmittableAction
{
    [JsonProperty("isSubmit")]
    public bool IsSubmit { get; set; }
    
    /// <summary>
    /// Comando SQL a ser executado, aceita expression
    /// </summary>
    [JsonProperty("commandSQL")]
    public string SqlCommand { get; set; }

    /// <summary>
    /// Aplicar somenter as linhas selecionadas (default=false)
    /// </summary>
    /// <remarks>
    /// Valido somente para contexto da toolbar, 
    /// onde o comando é aplicado para cada linha selecionada
    /// </remarks>
    [JsonProperty("applyOnSelected")]
    public bool ApplyOnSelected { get; set; }

    public SqlCommandAction()
    {
        Icon = IconType.Play;
    }
}