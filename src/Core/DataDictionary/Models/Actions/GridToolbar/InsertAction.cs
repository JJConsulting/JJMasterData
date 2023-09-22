using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions;


public class InsertAction : GridToolbarAction
{
    public const string ActionName = "insert";

    /// <summary>
    /// Nome do dicionário utilizado para exibir a pré seleção de um registro (default= null)
    /// </summary>
    /// <remarks>
    /// Quando essa propriedade é configurada o sistema exibirá uma lista de registros para seleção. 
    /// Ao selecionar um registro o metodo de insert é disparado enviando os valores da seleção atual. 
    /// Se o metodo de insert for executado com sucesso o usuário será redirecionado para edição do dicionário do contexto.
    /// <para/>Importante! Ao executar o insert o id gerado automaticamente deverá ser retornado na procedure de set.
    /// </remarks>
    [JsonProperty("elementNameToSelect")]
    public string ElementNameToSelect { get; set; }

    /// <summary>
    /// Reabrir o formulário (default=false)
    /// </summary>
    /// <remarks>
    /// False = Fecha o formulário e volta para lista
    /// True = Reabre o formulário para inserir um novo registro
    /// </remarks>
    [JsonProperty("reopenForm")]
    public bool ReopenForm { get; set; }

    [JsonProperty("showAsModal")]
    public bool ShowAsModal { get; set; }
    
    public InsertAction()
    {
        Name = ActionName;
        Text = "New";
        Icon = IconType.PlusCircle;
        ShowAsButton = true;
        Order = 1;
    }
}