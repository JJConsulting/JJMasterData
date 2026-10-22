namespace JJConsulting.Html.Bootstrap.Models;

/// <summary>
/// Tipo de agrupamento de dados
/// </summary>
public enum PanelLayout
{
    NoDecoration = 0,
    Tab = 1,
    Collapse = 2,
    Card = 3,
    [Obsolete("Please use Card.")]
    Panel = 3,
    [Obsolete("Please use Card.")]
    Well = 4
}