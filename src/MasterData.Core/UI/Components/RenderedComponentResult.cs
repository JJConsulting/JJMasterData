#nullable enable

using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public sealed class RenderedComponentResult(HtmlBuilder htmlBuilder) : HtmlComponentResult(htmlBuilder);