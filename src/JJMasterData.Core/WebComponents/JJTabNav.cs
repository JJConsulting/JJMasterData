using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Language;

namespace JJMasterData.Core.WebComponents;

public class JJTabNav : JJBaseView
{
    private int? _SelectedTabIndex;
    public int SelectedTabIndex
    {
        get
        {

            if (_SelectedTabIndex == null)
            {
                string tabIndex = CurrentContext.Request["selected_tab_" + Name];
                if (int.TryParse(tabIndex, out int nIndex))
                    _SelectedTabIndex = nIndex;
                else
                    _SelectedTabIndex = 0;

            }

            return (int)_SelectedTabIndex;
        }
        set
        {
            _SelectedTabIndex = value;
        }
    }

    public List<NavContent> ListTab { get; set; }

    public JJTabNav()
    {
        Name = "nav1";
        ListTab = new List<NavContent>();
    }

    protected override string RenderHtml()
    {
        char TAB = '\t';
        StringBuilder html = new();

        html.Append("<input type=\"hidden\" ");
        html.AppendFormat("id =\"selected_tab_{0}\" ", Name);
        html.AppendFormat("name =\"selected_tab_{0}\" ", Name);
        html.AppendFormat("value =\"{0}\"", SelectedTabIndex);
        html.AppendLine("/>");

        html.AppendLine("<ul class=\"nav nav-tabs\" role=\"tablist\"> ");

        for (int i = 0; i < ListTab.Count; i++)
        {
            NavContent nav = ListTab[i];
            string navId = Name + "_nav_" + i;

            html.Append(TAB, 1);
            if (SelectedTabIndex == i && BootstrapHelper.Version != 3)
                html.AppendFormat("<li class=\"active nav-item\">", navId);
            else
                html.AppendFormat("<li class=\"nav-item\">", navId);

            html.AppendFormat($"<a {BootstrapHelper.DataToggle}=\"tab\" href=\"#{0}\" tabindex=\"{1}\" class=\"nav-link {2}\">{3}</a>", navId, i, (SelectedTabIndex == i && BootstrapHelper.Version == 3 ? "active" : string.Empty), Translate.Key(nav.Title));
            html.AppendLine("</li>");

        }
        html.AppendLine("</ul> ");


        html.AppendLine("<div class=\"tab-content\" style=\"margin-top: 20px;\">");
        for (int i = 0; i < ListTab.Count; i++)
        {
            NavContent nav = ListTab[i];
            string navId = Name + "_nav_" + i;
            string tabClass = (SelectedTabIndex == i) ? "tab-pane fade in active" : "tab-pane fade";
            
            html.Append(TAB, 1);
            html.AppendFormat("<div id=\"{0}\" class=\"{1}\" role=\"tabpanel\">", navId, tabClass);
            html.AppendLine("");
            html.Append(nav.HtmlContent);
            html.Append(TAB, 1);
            html.AppendLine("</div>");
            
        }

        html.AppendLine("</div>");
        html.AppendLine("<br>");

        html.AppendLine("<script type=\"text/javascript\"> ");
        html.AppendLine("	$(document).ready(function () { ");
        html.AppendLine($"		 $(\"a[{BootstrapHelper.DataToggle}='tab']\").on(\"shown.bs.tab\", function (e) {{ ");
        html.AppendLine("			var target = $(e.target).attr(\"tabindex\");");
        html.AppendLine($"			$(\"#selected_tab_{Name}\").val(target);");
        html.AppendLine("		 });");
        html.AppendLine("	}); ");
        html.AppendLine("</script>");

        return html.ToString();
    }
}
