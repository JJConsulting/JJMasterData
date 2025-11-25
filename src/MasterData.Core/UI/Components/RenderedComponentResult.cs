#nullable enable

using JJConsulting.Html;


namespace JJMasterData.Core.UI.Components;

public sealed class RenderedComponentResult(HtmlBuilder htmlBuilder) : HtmlComponentResult(htmlBuilder);