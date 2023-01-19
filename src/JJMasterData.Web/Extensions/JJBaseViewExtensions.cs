using JJMasterData.Core.Web.Components;
using Microsoft.AspNetCore.Html;

namespace JJMasterData.Web.Extensions;

public static class JJBaseViewExtensions
{
    public static HtmlString GetHtmlString(this JJBaseView jjBaseView) => jjBaseView.GetHtml().ToHtmlString();
}