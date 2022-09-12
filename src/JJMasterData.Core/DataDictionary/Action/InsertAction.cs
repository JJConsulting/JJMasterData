using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
[DataContract]
public class InsertAction : BasicAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ACTION_NAME = "insert";

    /// <summary>
    /// Nome do dicionário utilizado para exibir a pré seleção de um registro (default= null)
    /// </summary>
    /// <remarks>
    /// Quando essa propriedade é configurada o sistema exibirá uma lista de registros para seleção. 
    /// Ao selecionar um registro o metodo de insert é disparado enviando os valores da seleção atual. 
    /// Se o metodo de insert for executado com sucesso o usuário será redirecionado para edição do dicionário do contexto.
    /// <para/>Importante! Ao executar o insert o id gerado automaticamente deverá ser retornado na procedure de set.
    /// </remarks>
    [DataMember(Name = "elementNameToSelect")]
    public string ElementNameToSelect { get; set; }

    /// <summary>
    /// Reabrir o formulário (default=false)
    /// </summary>
    /// <remarks>
    /// False = Fecha o formulário e volta para lista
    /// True = Reabre o formulário para inserir um novo registro
    /// </remarks>
    [DataMember(Name = "reopenForm")]
    public bool ReopenForm { get; set; }

    public InsertAction()
    {
        Name = ACTION_NAME;
        Text = "New";
        Icon = IconType.PlusCircle;
        ShowAsButton = true;
        Order = 1;
    }
}