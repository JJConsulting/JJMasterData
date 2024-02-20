using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Opções configuradas no dicionário de dados
/// </summary>

public class GridUI
{
    /// <summary>
    /// Total de Registros por página 
    /// (Default = 5)
    /// </summary>
    /// <remarks>
    /// Se o RecordsPerPage for zero a paginação não será exibida
    /// </remarks>
    [JsonProperty("totalPerPage")]
    [Display(Name = "Total Of Records per page")]
    public int RecordsPerPage { get; set; } = 5;

    /// <summary>
    /// Total de botões na paginação 
    /// (Default = 5)
    /// </summary>
    [JsonProperty("totalPaggingButton")]
    [Display(Name = "Total Pagination Buttons")]
    public int TotalPaggingButton { get; set; } = 5;

    /// <summary>
    /// Exibi borda na grid 
    /// (Default = false)
    /// </summary>
    [JsonProperty("showBorder")]
    [Display(Name = "Show Border")]
    public bool ShowBorder { get; set; }

    /// <summary>
    /// Exibir colunas zebradas 
    /// (Default = true)
    /// </summary>*
    [JsonProperty("showRowStriped")]
    [Display(Name = "Show Rows striped")]
    public bool ShowRowStriped { get; set; } = true;

    /// <summary>
    /// Alterar a cor da linha ao passar o mouse 
    /// (Default = true)
    /// </summary>
    [JsonProperty("showRowHover")]
    [Display(Name = "Show Row Hover")]
    public bool ShowRowHover { get; set; } = true;

    /// <summary>
    /// Quantidade total de registros existentes no banco
    /// </summary>
    [JsonProperty("totalReg")]
    [Display(Name = "Total Of Records")]
    public int TotalOfRecords { get; set; }

    /// <summary>
    /// Exibir título no cabeçalho da página
    /// </summary>
    [JsonProperty("showTitle")]
    [Display(Name = "Show Title")]
    public bool ShowTitle { get; set; } = true;

    /// <summary>
    /// Exibir toolbar (Default = true) 
    /// </summary>
    [JsonProperty("showToolbar")]
    [Display(Name = "Show Toolbar")]
    public bool ShowToolBar { get; set; } = true;

    /// <summary>
    /// Habilita Ordenação das colunas (Default = true)
    /// </summary>
    /// <remarks>
    /// Habilita ou não o link nos titulos permitindo a ordenação.
    /// Mesmo quando configurado como falso, a grid respeita a propriedade CurrentOrder
    /// </remarks>
    [JsonProperty("enableSorting")]
    [Display(Name = "Enable Sorting")]
    public bool EnableSorting { get; set; } = true;

    /// <summary>
    /// Permite selecionar multiplas linhas na Grid 
    /// habilitando um checkbox na primeira coluna. (Defaut = false)
    /// </summary>
    [JsonProperty("enableMultSelect")]
    [Display(Name = "Enable Multiselect")]
    public bool EnableMultiSelect { get; set; }

    /// <summary>
    /// Maintains filters, order and grid pagination in the session,
    /// and recovers on the first page load. (Default = false)
    /// </summary>
    /// <remarks>
    /// When using this property, we recommend changing the object's [Name] parameter.
    /// The [Name] property is used to compose the name of the session variable.
    /// </remarks>
    [JsonProperty("maintainValuesOnLoad")]
    [Display(Name = "Save User Preferences on Session")]
    public bool MaintainValuesOnLoad { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the GridView header will remain visible when there is no data.
    /// </summary>
    /// <remarks>
    /// Default value = (True).
    /// <para/>
    /// To change the empty data message, refer to the EmptyDataText property
    /// </remarks>
    [JsonProperty("showHeaderWhenEmpty")]
    [Display(Name = "Show Header When Empty")]
    public bool ShowHeaderWhenEmpty { get; set; } = true;

    /// <summary>
    /// Gets or sets the text to be displayed in the empty data row when a JJGridView control contains no records.
    /// </summary>
    /// <remarks>
    /// Default value = (There is no record to be displayed).
    /// <para/>
    /// </remarks>
    [JsonProperty("emptyDataText")]
    [Display(Name = "Empty Data Text")]
    public string EmptyDataText { get; set; } = "There is no record to display.";

    /// <summary>
    /// Displays pagination controls (Default = true)
    /// </summary>
    /// <remarks>
    /// Hides all pagination buttons, however, it maintains the pre-defined pagination controls.
    /// <para/>
    /// Pagination will be displayed if the number of records in the grid exceeds the minimum number of records on a page.
    /// <para/>
    /// If the CurrentPage property is equal to zero, pagination will not be displayed.
    /// <para/>
    /// If the CurrentUI.RecordsPerPage property is equal to zero, pagination will not be displayed.
    /// <para/>
    /// If the TotalRecords property is equal to zero, pagination will not be displayed.
    /// </remarks>
    [JsonProperty("showPagging")]
    [Display(Name = "Enable Pagination")]
    public bool ShowPagging { get; set; } = true;
    
    [JsonProperty("headerFixed")]
    [Display(Name = "Header Fixed At Scroll")]
    public bool HeaderFixed { get; set; }

    [JsonProperty("useVerticalLayoutAtFilter")]
    [Display(Name = "Use Vertical Layout At Filter")]
    public bool UseVerticalLayoutAtFilter { get; set; } 
    
    [JsonProperty("isCompact")]
    [Display(Name = "Compact mode")]
    public bool IsCompact { get; set; }
}