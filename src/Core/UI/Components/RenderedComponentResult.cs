#nullable enable

using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class RenderedComponentResult(HtmlBuilder htmlBuilder) : HtmlComponentResult(htmlBuilder);