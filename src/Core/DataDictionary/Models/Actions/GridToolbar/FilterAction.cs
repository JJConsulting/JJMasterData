using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

/// <summary>
/// Represents the dictionary export button.
/// </summary>
public class FilterAction : GridToolbarAction
{
    /// <summary>
    /// Action default name.
    /// </summary>
    public const string ActionName = "filter";
    

    /// <summary>
    /// Exibir em um collapse painel.
    /// <para></para>(Default = true)
    /// </summary>
    /// <remarks>
    /// Quando essa propriedade é habilitada, o icone de filtro será removido da toolbar da grid 
    /// e um painel com os filtros será exibido acima da grid. 
    /// O comportamento do filtro permanece o mesmo.
    /// </remarks>
    [JsonProperty("showAsCollapse")]
    public bool ShowAsCollapse { get; set; }

    /// <summary>
    /// Exibir o collapse painel aberto por padrão.
    /// Aplícavél somente se a propriedade ShowAsCollapse estiver habilitada
    /// <para></para>(Default = false)
    /// </summary>
    [JsonProperty("expandedByDefault")]
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
    [JsonProperty("enableScreenSearch")]
    public bool EnableScreenSearch { get; set; }

    public FilterAction()
    {
        Name = ActionName;
        ToolTip = "Filter";
        Icon = IconType.Binoculars;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 10;
        ShowAsCollapse = true;
        ExpandedByDefault = false;
        EnableScreenSearch = false;
    }
}