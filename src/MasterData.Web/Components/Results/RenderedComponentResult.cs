using JJConsulting.Html;


namespace JJMasterData.Web.Components;

public sealed class RenderedComponentResult(HtmlBuilder htmlBuilder) : HtmlComponentResult(htmlBuilder);