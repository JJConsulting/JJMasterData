namespace JJMasterData.Core.Html;

/// <summary>
/// Default HTML tags layouts.
/// </summary>
internal static class Layouts
{
    /// <summary>
    /// Layout for start and end tag.
    /// </summary>
    internal const string StartEndTagLayout = "<{0}{1}>{{0}}</{0}>";

    /// <summary>
    /// Layout for single tag.
    /// </summary>
    internal const string SingleTagLayout = "<{0}{1}/>";

    /// <summary>
    /// Layout for attribute.
    /// </summary>
    internal const string AttributeLayout = " {0}=\"{1}\"";
}