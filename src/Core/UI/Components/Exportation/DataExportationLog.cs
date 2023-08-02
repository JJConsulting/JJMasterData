using JJMasterData.Commons.Localization;
using JJMasterData.Core.Web.Components.Scripts;
using JJMasterData.Core.Web.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

internal class DataExportationLog
{
    private DataExportationScripts Scripts { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private readonly string _componentName;
    private readonly bool _isExternalRoute;
    private readonly string _dictionaryName;

    public DataExportationLog(JJDataExportation dataExportation)
    {
        Scripts = dataExportation.Scripts;
        StringLocalizer = dataExportation.StringLocalizer;
        _componentName = dataExportation.Name;
        _isExternalRoute = dataExportation.IsExternalRoute;
        _dictionaryName = dataExportation.FormElement.Name;
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
                span.WithAttribute("id", "lblStartDate");
            });
            
            div.Append(HtmlTag.Br);
            div.Append(HtmlTag.Br);
            div.Append(HtmlTag.Br);

            div.Append(HtmlTag.A, a =>
            {
                var stopExportationScript = Scripts.GetStopExportationScript(_dictionaryName, _componentName,
                    StringLocalizer["Stopping Processing..."], _isExternalRoute);
                a.WithAttribute("href", $"javascript:{stopExportationScript}");
                a.Append(HtmlTag.Span, span =>
                {
                    span.WithCssClass("fa fa-stop");
                });
                a.AppendText("&nbsp;" + StringLocalizer["Stop the exportation."]);
            });
        });
        return div;
    }

    private static HtmlBuilder GetLoading()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", "divProcess")
            .WithAttribute("style", "text-align:center;")
            .AppendHiddenInput("current_uploadaction", string.Empty)
            .Append(HtmlTag.Div, div =>
            {
                div.WithAttribute("id", "exportationSpinner");
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