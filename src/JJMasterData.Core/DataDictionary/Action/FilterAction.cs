using System;
using System.Runtime.Serialization;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.DataDictionary.Action;

/// <summary>
/// Represents the dictionary export button.
/// </summary>
[Serializable]
[DataContract]
public class FilterAction : BasicAction
{
    /// <summary>
    /// Action default name.
    /// </summary>
    public const string ACTION_NAME = "filter";

    /// <summary>
    /// Exibir em um collapse painel.
    /// <para></para>(Default = true)
    /// </summary>
    /// <remarks>
    /// Quando essa propriedade é habilitada, o icone de filtro será removido da toolbar da grid 
    /// e um painel com os filtros será exibido acima da grid. 
    /// O comportamento do filtro permanece o mesmo.
    /// </remarks>
    [DataMember(Name = "showAsCollapse")]
    public bool ShowAsCollapse { get; set; }

    /// <summary>
    /// Exibir o collapse painel aberto por padrão.
    /// Aplícavél somente se a propriedade ShowAsCollapse estiver habilitada
    /// <para></para>(Default = false)
    /// </summary>
    [DataMember(Name = "expandedByDefault")]
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
    [DataMember(Name = "enableScreenSearch")]
    public bool EnableScreenSearch { get; set; }

    public FilterAction()
    {
        Name = ACTION_NAME;
        ToolTip = "Filter";
        Icon = IconType.Binoculars;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 10;
        ShowAsCollapse = true;
        ExpandedByDefault = false;
        EnableScreenSearch = false;
    }
}