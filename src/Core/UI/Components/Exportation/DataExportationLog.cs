using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class DataExportationLog(JJDataExportation dataExportation)
{
    private readonly DataExportationScripts _scripts  = dataExportation.Scripts;
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer = dataExportation.StringLocalizer;

    internal HtmlBuilder GetLoadingHtml()
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        
        div.WithCssClass("text-center");
        
        div.Append(GetLoading());

        div.Append(HtmlTag.Div, div =>
        {
            div.WithAttribute("id", "process-status")
                .WithCssClass("text-center")
                .WithStyle( "display:none;");
            
            div.Append(GetProgressData());

            div.Append(HtmlTag.Br);

            div.AppendText(_stringLocalizer["Exportation started on"]);

            div.AppendText(" ");
            
            div.Append(HtmlTag.Span, span => span.WithAttribute("id", "start-date-label"));
            
            div.Append(HtmlTag.Br);
            div.WithCssClass("mb-1");
            var stopExportationScript = _scripts.GetStopExportationScript(_stringLocalizer["Stopping Processing..."]);
            var reporter = dataExportation.BackgroundTaskManager.GetProgress<DataExportationReporter>(dataExportation.ProcessKey);
            div.AppendComponent(new JJLinkButton
            {
                IconClass = "fa fa-stop",
                OnClientClick = stopExportationScript,
                Visible = reporter?.UserId == dataExportation.CurrentContext.User.GetUserId(),
                Text = _stringLocalizer["Stop the exportation"],
                ShowAsButton = true
            });
        });

        div.AppendScript(_scripts.GetStartProgressVerificationScript());
        
        return div;
    }

    private static HtmlBuilder GetLoading()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", "divProcess")
            .WithStyle( "text-align:center;")
            .Append(HtmlTag.Div, div =>
            {
                div.WithAttribute("id", "data-exportation-spinner-");
                div.WithStyle( "position: relative; height: 80px");
            });
    }

    private static HtmlBuilder GetProgressData()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .Append(HtmlTag.Div, div =>
            {
                div.WithAttribute("id", "divStatus");
                div.WithCssClass("text-center");
                div.Append(HtmlTag.Span, span => span.WithAttribute("id", "process-message"));
            })
            .Append(HtmlTag.Div, div =>
            {
                div.WithStyle( "display:none;width:50%");
                div.WithCssClass(BootstrapHelper.CenterBlock);
                div.Append(HtmlTag.Div, div =>
                {
                    div.WithCssClass("progress");
                    div.Append(HtmlTag.Div, div =>
                    {
                        div.WithCssClass("progress-bar");
                        div.WithAttribute("role", "progressbar");
                        div.WithStyle( "width: 0;");
                        div.WithAttribute("aria-valuemin", "0");
                        div.WithAttribute("aria-valuemax", "100");
                        div.AppendText("0%");
                    });
                });
            });
    }
}