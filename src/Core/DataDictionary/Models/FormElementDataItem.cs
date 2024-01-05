#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JJMasterData.Commons.Data;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Configurações de objetos do tipo lista
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>

public class FormElementDataItem 
{
    [JsonProperty("dataItemType")]
    public DataItemType DataItemType { get; set; }

    /// <summary>
    /// Command executed to recover DataItemValues. Returns two columns:
    /// 1) Id;
    /// 2) Description.
    /// </summary>
    [JsonProperty("command")]
    public DataAccessCommand? Command { get; set; }
    
    [JsonProperty("itens")]
    public IList<DataItemValue>? Items { get; set; }
    
    /// <summary>
    /// Relationship with another Element to recover values
    /// </summary>
    [JsonProperty("elementMap")]
    public DataElementMap? ElementMap { get; set; }

    /// <summary>
    /// Exibir texto (Todos) como primeira opção (Default = NONE)
    /// </summary>
    [JsonProperty("firstoption")]
    public FirstOptionMode FirstOption { get; set; } = FirstOptionMode.None;

    /// <summary>
    /// Replace the field value with the description or description/icon when displaying the grid (Default = true)
    /// </summary>
    /// <remarks>
    /// If you use a field filter in the query ({FIELD}), the text may be displayed incorrectly in the grid.
    /// The system caches the combo data from the first record displayed in the list.<br></br>
    /// In this case, for performance reasons, we recommend disabling this option and
    /// create a new field of type VIEWONLY to handle the result in the procedure
    /// </remarks>
    [JsonProperty("replacetextongrid")]
    [Display(Name="Replace On Grid")]
    public bool ReplaceTextOnGrid { get; set; } = true;

    /// <remarks>
    /// Be careful when using this option. You should probably use this option only for WriteOnly fields or store the values in another table.
    /// </remarks>
    [JsonProperty("enableMultiSelect")]
    [Display(Name="Enable Multi Select")]
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
    [Display(Name="Show Icon")]
    public bool ShowIcon { get; set; }

    public bool HasSqlCommand() => !string.IsNullOrWhiteSpace(Command?.Sql);

    public bool HasElementMap()
    {
        return ElementMap != null;
    }

    public bool HasItems() => Items?.Count > 0;
}