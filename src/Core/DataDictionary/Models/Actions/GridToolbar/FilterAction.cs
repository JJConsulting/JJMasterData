using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Represents the dictionary export button.
/// </summary>
public sealed class FilterAction : GridToolbarAction
{
    public const string ActionName = "filter";
    
    /// <summary>
    /// Display in a collapsible panel.
    /// <para></para>(Default = true)
    /// </summary>
    /// <remarks>
    /// When this property is enabled, the filter icon will be removed from the grid's toolbar,
    /// and a panel with filters will be displayed above the grid.
    /// The filter's behavior remains the same.
    /// </remarks>
    [JsonPropertyName("showAsCollapse")]
    [Display(Name = "Show as Collapse")]
    public bool ShowAsCollapse { get; set; }
    
    /// <summary>
    /// Exibir o collapse painel aberto por padrão.
    /// Aplícavél somente se a propriedade ShowAsCollapse estiver habilitada
    /// <para></para>(Default = false)
    /// </summary>
    [JsonPropertyName("expandedByDefault")]
    [Display(Name = "Expanded By Default")]
    public bool ExpandedByDefault { get; set; }

    /// <summary>
    /// Habilitar pesquisa rápida na tela.<br></br> 
    /// Essa pesquisa não consulta o banco de dados, apenas procura os registros exibidos na grid.<br></br>
    /// Normalmente utilizada quando a paginação da grid esta desabilitada.
    /// <para></para>(Default = false)
    /// </summary>
    /// <remarks>
    /// Quando essa propriedade é habilitada todos os campos de filtro serão ignorados 
    /// e apenas um campo texto será aprensentado para o usuário como opção de filtro.<br></br>
    /// Se a propriedade ShowAsCollapse estiver desabilitada o campo de texto será exibido 
    /// na Toolbar junto com os botões de acão respeitando a ordem configurada.
    /// </remarks>
    [JsonPropertyName("enableScreenSearch")]
    [Display(Name = "Enable Client-Side Search")]
    public bool EnableScreenSearch { get; set; }

    public FilterAction()
    {
        Name = ActionName;
        Tooltip = "Filter";
        Text = "Detailed Filters";
        Icon = IconType.Filter;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 10;
        ShowAsCollapse = true;
        ExpandedByDefault = false;
        EnableScreenSearch = false;
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}