using JJMasterData.Commons.Localization;
using JJMasterData.Core.Web.Components.Scripts;
using JJMasterData.Core.Web.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

internal class DataExportationLog
{
    private DataExportationScripts Scripts { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public DataExportationLog(JJDataExportation dataExportation)
    {
        Scripts = dataExportation.Scripts;
        StringLocalizer = dataExportation.StringLocalizer;
    }

    internal HtmlBuilder GetHtmlProcess()
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        
        div.WithCssClass("text-center");
        
        div.Append(GetLoading());

        div.Append(HtmlTag.Div, div =>
        {
            div.WithAttribute("id", "divMsgProcess")
                .WithCssClass("text-center")
                .WithAttribute("style", "display:none;");
            
            div.Append(GetProgressData());

            div.Append(HtmlTag.Br);

            div.AppendText(StringLocalizer["Exportation started on"]);

            div.AppendText(" ");
            
            div.Append(HtmlTag.Span, span =>
            {
                span.WithAttribute("id", "start-date-label");
            });
            
            div.Append(HtmlTag.Br);
            div.Append(HtmlTag.Br);
            div.Append(HtmlTag.Br);

            div.Append(HtmlTag.A, a =>
            {
                var stopExportationScript = Scripts.GetStopExportationScript(StringLocalizer["Stopping Processing..."]);
                a.WithAttribute("href", $"javascript:{stopExportationScript}");
                a.Append(HtmlTag.Span, span =>
                {
                    span.WithCssClass("fa fa-stop");
                });
                a.AppendText($"&nbsp;{StringLocalizer["Stop the exportation."]}");
            });
        });
        return div;
    }

    private static HtmlBuilder GetLoading()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", "divProcess")
            .WithAttribute("style", "text-align:center;")
            .Append(HtmlTag.Div, div =>
            {
                div.WithAttribute("id", "data-exportation-spinner");
                div.WithAttribute("style", "position: relative; height: 80px");
            });
    }

    private static HtmlBuilder GetProgressData()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .Append(HtmlTag.Div, div =>
            {
                div.WithAttribute("id", "divStatus");
                div.WithCssClass("text-center");
                div.Append(HtmlTag.Span, span => { span.WithAttribute("id", "lblResumeLog"); });
            })
            .Append(HtmlTag.Div, div =>
            {
                div.WithAttribute("style", "display:none;width:50%");
                div.WithCssClass(BootstrapHelper.CenterBlock);
                div.Append(HtmlTag.Div, div =>
                {
                    div.WithCssClass("progress");
                    div.Append(HtmlTag.Div, div =>
                    {
                        div.WithCssClass("progress-bar");
                        div.WithAttribute("role", "progressbar");
                        div.WithAttribute("style", "width: 0;");
                        div.WithAttribute("aria-valuemin", "0");
                        div.WithAttribute("aria-valuemax", "100");
                        div.AppendText("0%");
                    });
                });
            });
    }
}