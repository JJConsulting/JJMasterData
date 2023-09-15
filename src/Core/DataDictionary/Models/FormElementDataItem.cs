#nullable enable
using System.Collections.Generic;
using System.Runtime.Serialization;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Validations;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Configurações de objetos do tipo lista
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>

public class FormElementDataItem 
{
    private DataAccessCommand? _command;
    private IList<DataItemValue>? _items;
    
    /// <summary>
    /// Tipo da origem dos dados
    /// </summary>
    [JsonProperty("dataItemType")]
    public DataItemType DataItemType { get; set; }

    /// <summary>
    /// Command executed to recover DataItemValues. Returns two columns:
    /// 1) Id;
    /// 2) Description.
    /// </summary>
    [DataMember(Name = "command")]
    [RequiredIf(nameof(Items), null)]
    public DataAccessCommand? Command
    {
        get => _command ??= new DataAccessCommand();
        set => _command = value;
    }
    
    [DataMember(Name = "itens")]
    [RequiredIf(nameof(Command), null)]
    public IList<DataItemValue>? Items
    {
        get => _items ??= new List<DataItemValue>();
        set => _items = value;
    }


    /// <summary>
    /// Mapeamento do dicionário
    /// </summary>
    [JsonProperty("elementMap")]
    public DataElementMap? ElementMap { get; set; } 

    /// <summary>
    /// Exibir texto (Todos) como primeira opção (Default = NONE)
    /// </summary>
    [JsonProperty("firstoption")]
    public FirstOptionMode FirstOption { get; set; } = FirstOptionMode.None;

    /// <summary>
    /// Substituir o valor do campo com a descrição da combo ao exibir a grid (Default = true)
    /// </summary>
    /// <remarks>
    /// Se utilizar filtro de campos na query ({CAMPO}), o texto pode ser exibido incorretamente na grid.
    /// O sistema faz cache dos dados da combo do primeiro registro exibido na lista.<br></br>
    /// Nesse caso por questão de desempenho recomendamos desabilitar essa opção e 
    /// criar um novo campo do tipo VIEWONLY para tratar o resultado na procedure
    /// </remarks>
    [JsonProperty("replacetextongrid")]
    public bool ReplaceTextOnGrid { get; set; } = true;

    /// <remarks>
    /// Be careful when using this option. You should probably use this option only for WriteOnly fields or store the values in another table.
    /// </remarks>
    [JsonProperty("enableMultiSelect")]
    public bool EnableMultiSelect { get; set; }

    /// <summary>
    /// Permite incluir imagens na lista como legendas
    /// </summary>
    /// <remarks>
    /// Exibir como Legenda inclui imagem nos componentes combobox e na grid.<para></para>
    /// Para exibir apenas a imagem de status na grid certifique-se a 
    /// propriedade ReplaceTextOnGrid esta configurada para falso.
    /// </remarks>
    [JsonProperty("showimagelegend")]
    public bool ShowImageLegend { get; set; }

    public bool HasSqlExpression()
    {
        if (Command == null)
            return false;

        if (string.IsNullOrEmpty(Command.Sql))
            return false;

        string sql = Command.Sql;
        sql = sql.Replace("{search_id}", string.Empty);
        sql = sql.Replace("{search_text}", string.Empty);
        return sql.Contains("{");
    }
}