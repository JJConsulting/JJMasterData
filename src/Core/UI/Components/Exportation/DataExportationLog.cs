using JJMasterData.Commons.Localization;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class DataExportationLog(JJDataExportation dataExportation)
{
    private DataExportationScripts Scripts { get; } = dataExportation.Scripts;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = dataExportation.StringLocalizer;

    internal HtmlBuilder GetLoadingHtml()
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        
        div.WithCssClass("text-center");
        
        div.Append(GetLoading());

        div.Append(HtmlTag.Div, div =>
        {
            div.WithAttribute("id", "process-status")
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
            div.WithCssClass("mb-1");
            var stopExportationScript = Scripts.GetStopExportationScript(StringLocalizer["Stopping Processing..."]);
            div.AppendComponent(new JJLinkButton(StringLocalizer)
            {
                IconClass = "fa fa-stop",
                OnClientClick = stopExportationScript,
                Text = "Stop the exportation",
                ShowAsButton = true
            });
        });

        div.AppendScript(Scripts.GetStartProgressVerificationScript());
        
        return div;
    }

    private static HtmlBuilder GetLoading()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", "divProcess")
            .WithAttribute("style", "text-align:center;")
            .Append(HtmlTag.Div, div =>
            {
                div.WithAttribute("id", "data-exportation-spinner-");
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
                div.Append(HtmlTag.Span, span => { span.WithAttribute("id", "process-message"); });
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