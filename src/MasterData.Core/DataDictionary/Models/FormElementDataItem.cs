#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JJMasterData.Commons.Data;


namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Configurações de objetos do tipo lista
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
public class FormElementDataItem
{
    [JsonPropertyName("dataItemType")] 
    public DataItemType DataItemType { get; set; }

    /// <summary>
    /// Command executed to recover DataItemValues. Returns two columns:
    /// 1) Id;
    /// 2) Description.
    /// </summary>
    [JsonPropertyName("command")]
    public DataAccessCommand? Command { get; set; }

    [JsonPropertyName("itens")] 
    public List<DataItemValue>? Items { get; set; }

    /// <summary>
    /// Relationship with another Element to recover values
    /// </summary>
    [JsonPropertyName("elementMap")]
    public DataElementMap? ElementMap { get; set; }

    /// <summary>
    /// Exibir texto (Todos) como primeira opção (Default = NONE)
    /// </summary>
    [JsonPropertyName("firstoption")]
    [Display(Name = "First Option")]
    public FirstOptionMode FirstOption { get; set; } = FirstOptionMode.None;

    [JsonPropertyName("radioLayout")]
    [Display(Name = "Layout")]
    public DataItemRadioLayout? RadioLayout { get; set; }
    
    /// <summary>
    /// Enables localization of list item descriptions.
    /// </summary>
    [JsonPropertyName("enableLocalization")]
    [Display(Name = "Enable Localization")]
    public bool EnableLocalization { get; set; }

    /// <remarks>
    /// Be careful when using this option. You should probably use this option only for WriteOnly fields or store the values in another table.
    /// </remarks>
    [JsonPropertyName("enableMultiSelect")]
    [Display(Name = "Enable Multi Select")]
    public bool EnableMultiSelect { get; set; }

    [Display(Name = "Grid Behavior")]
    [JsonPropertyName("gridBehavior")]
    public DataItemGridBehavior GridBehavior { get; set; }

    /// <summary>
    /// Permite incluir imagens na lista como legendas
    /// </summary>
    /// <remarks>
    /// Exibir como Legenda inclui imagem nos componentes combobox e na grid.<para></para>
    /// Para exibir apenas a imagem de status na grid certifique-se a 
    /// propriedade ReplaceTextOnGrid esta configurada para falso.
    /// </remarks>
    [JsonPropertyName("showimagelegend")]
    [Display(Name = "Show Icon")]
    public bool ShowIcon { get; set; }

    public bool SupportsFloatingLabels() => !EnableMultiSelect && !ShowIcon;

    public bool HasSqlCommand() => !string.IsNullOrWhiteSpace(Command?.Sql);

    public bool HasElementMap() => ElementMap != null;

    public bool HasItems() => Items?.Count > 0;

    public FormElementDataItem DeepCopy()
    {
        var copy = (FormElementDataItem)MemberwiseClone();
        
        copy.Command = Command?.DeepCopy();
        copy.Items = Items?.ConvertAll(i => i.DeepCopy());
        copy.ElementMap = ElementMap?.DeepCopy();
        
        return copy;
    }

}
