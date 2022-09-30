using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

public enum Positon
{
    Right = 0,
    Left = 1
}

public class JJCollapsePanel : JJBaseView
{
    public Positon PositonButton { get; set; }

    public string Title { get; set; }

    public JJIcon TitleIcon { get; set; }
    
    public string HtmlContent { get; set; }

    public List<JJLinkButton> Buttons { get; set; }

    public bool ExpandedByDefault { get; set; }

    public PanelColor Color { get; set; }

    private bool IsCollapseOpen
    {
        get
        {
            string collapseMode = CurrentContext.Request["collapse_mode_" + Name];
            if (string.IsNullOrEmpty(collapseMode))
                return ExpandedByDefault;
            return "1".Equals(collapseMode);
        }
    }

    public JJCollapsePanel()
    {
        PositonButton = Positon.Right;
        Name = "collapse1";
        Buttons = new List<JJLinkButton>();
        Color = PanelColor.Default;
        TitleIcon = null;
    }

    internal override HtmlElement GetHtmlElement()
    {
        var root = new HtmlElement(HtmlTag.Div);
        root.AppendElement(HtmlTag.Input, input =>
        {
            input.WithAttribute("hidden", "hidden");
            input.WithNameAndId($"collapse_mode_{Name}");
            input.WithValue(IsCollapseOpen ? "1" : "0");
        });
        root.AppendElement(HtmlTag.Div, div =>
        {
            div.WithCssClass(BootstrapHelper.PanelGroup);
            div.WithCssClass(BootstrapHelper.GetPanel(Color.ToString().ToLower()));
            div.AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass(BootstrapHelper.GetPanelHeading(Color.ToString().ToLower()));
                div.WithHref("#collapseOne");
                div.WithAttribute("data-toggle", "collapse");
                div.WithAttribute("data-target", "#" + Name);
                div.WithAttribute("", IsCollapseOpen.ToString().ToLower());
                
            });
        });
    }
    
    protected override string RenderHtml()
    {
        char TAB = '\t';
        StringBuilder html = new ();
        string title = Translate.Key(Title);
        if (BootstrapHelper.Version < 5)
        {
            html.Append("<input type=\"hidden\" id=\"collapse_mode_");
            html.Append(Name);
            html.Append("\" name=\"collapse_mode_");
            html.Append(Name);
            html.Append("\" value=\"");
            html.Append(IsCollapseOpen ? "1" : "0");
            html.AppendLine("\" />");

            html.AppendLine($"<div class=\"{BootstrapHelper.PanelGroup}\"> ");
            html.Append(TAB, 1);


            html.AppendFormat($"<div class=\"{BootstrapHelper.GetPanel(Color.ToString().ToLower())}\">");


            html.AppendLine("");
            html.Append(TAB, 2);
            html.Append($"<div class=\"{BootstrapHelper.GetPanelHeading(Color.ToString().ToLower())}\" href=\"#collapseOne\" data-toggle=\"collapse\" data-target=\"#");
            html.Append(Name);
            html.Append("\" aria-expanded=\"");
            html.Append(IsCollapseOpen.ToString().ToLower());
            html.AppendLine("\"> ");
            html.Append(TAB, 3);
            html.AppendLine($"<div class=\"{BootstrapHelper.PanelTitle} unselectable\">");

            html.Append(TAB, 4);
            html.Append("<a>");
            if (TitleIcon != null)
            {
                html.Append(TitleIcon.GetHtml());
                html.Append("&nbsp;");
            }
            html.Append(title);
            html.AppendLine("</a>");

            html.Append(TAB, 3);
            html.AppendLine("</div>");
            html.Append(TAB, 2);
            html.AppendLine("</div> ");
            html.Append(TAB, 3);
            html.Append("<div id=\"");
            html.Append(Name);
            if (BootstrapHelper.Version == 3)
            {
                html.Append("\" class=\"panel-collapse collapse ");
                if (IsCollapseOpen)
                    html.Append("in ");
            }
            else
            {
                html.Append($"\" class=\"panel-collapse in collapse {(IsCollapseOpen ? "show" : string.Empty)} ");
            }
            html.AppendLine("\" > ");
            html.Append(TAB, 3);
            html.AppendLine($"<div class=\"{BootstrapHelper.PanelBody} {CssClass}\">");
            html.AppendLine(HtmlContent);
            html.Append(TAB, 4);
            html.AppendLine("<div class=\"row\"> ");
            html.Append(TAB, 5);

            if (Positon.Left == (PositonButton))
                html.AppendLine("<div class=\"col-md-12 text-left\"> ");
            else
                html.AppendLine("<div class=\"col-md-12 text-right\"> ");

            foreach (JJLinkButton btn in Buttons)
            {
                html.Append(TAB, 6);
                html.AppendLine(btn.GetHtml());
            }
            html.Append(TAB, 5);
            html.AppendLine("</div> ");
            html.Append(TAB, 4);
            html.AppendLine("</div> ");

            html.Append(TAB, 3);
            html.AppendLine("</div> ");
            html.Append(TAB, 2);
            html.AppendLine("</div> ");
            html.Append(TAB, 1);
            html.AppendLine("</div> ");

            html.AppendLine("</div> ");

            html.AppendLine("<script type=\"text/javascript\"> ");
            html.AppendLine(" ");
            html.AppendLine("	$(document).ready(function () { ");
            html.AppendLine("		$('#" + Name + "').on('hidden.bs.collapse', function () { ");
            html.AppendLine("			$(\"#collapse_mode_" + Name + "\").val(\"0\"); ");
            html.AppendLine("		}); ");
            html.AppendLine(" ");
            html.AppendLine("		$('#" + Name + "').on('show.bs.collapse', function () { ");
            html.AppendLine("			$(\"#collapse_mode_" + Name + "\").val(\"1\"); ");
            html.AppendLine("		}); ");
            html.AppendLine("	}); ");
            html.AppendLine("</script>");

            
        }
        else
        {
            html.AppendLine($"<div class=\"accordion pb-1\" id=\"{Name.ToLower()}-accordion\">");
            html.AppendLine("    <div class=\"accordion-item\">");
            html.AppendLine($"        <h2 class=\"accordion-header bg-{Color.ToString().ToLower().Replace("default", "jjmasterdata")}\" id=\"heading-{Name.ToLower()}\">");
            html.Append($"            <button class=\"accordion-button {(!IsCollapseOpen ? "collapsed" : "")}\" type=\"button\" data-bs-toggle=\"collapse\" data-bs-target=\"#collapse-{Name.ToLower()}\"");
            html.AppendLine($"aria-expanded=\"{IsCollapseOpen.ToString().ToLower()}\" aria-controls=\"collapseOne\">");
            html.AppendLine($"                {title}");
            html.AppendLine("            </button>");
            html.AppendLine("        </h2>");
            html.AppendLine($"        <div id=\"collapse-{Name.ToLower()}\" class=\"accordion-collapse collapse {(IsCollapseOpen ? "show" : "")}\" aria-labelledby=\"heading-{Name.ToLower()}\" data-bs-parent=\"#{Name.ToLower()}-accordion\">");
            html.AppendLine("            <div class=\"accordion-body\">");
            html.AppendLine(HtmlContent);
            html.AppendLine("            <div class=\"row\"> ");

            if (Positon.Left == (PositonButton))
                html.AppendLine("<div class=\"col-md-12 text-start\"> ");
            else
                html.AppendLine("<div class=\"col-md-12 text-end\"> ");
            foreach (JJLinkButton btn in Buttons)
            {
                html.Append(TAB, 6);
                html.AppendLine(btn.GetHtml());
            }
            html.AppendLine("</div> ");
            html.AppendLine("</div> ");
            html.AppendLine("            </div>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            html.AppendLine("</div>");
        }
        return html.ToString();
    }
}
