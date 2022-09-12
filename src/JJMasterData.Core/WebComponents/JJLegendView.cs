using System;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.WebComponents;

public class JJLegendView : JJBaseView
{
    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }

    public bool ShowAsModal { get; set; }

    #region "Constructors"

    public JJLegendView(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), "Nome do dicionário nao pode ser vazio");

        var dicParser = GetDictionary(elementName);
        FormElement = dicParser.GetFormElement();
        DoConstructor();
    }

    public JJLegendView(FormElement formElement)
    {
        FormElement = formElement;
        DoConstructor();
    }

    public JJLegendView(FormElement formElement, IDataAccess dataAccess) : base(dataAccess)
    {
        FormElement = formElement;
        DoConstructor();
    }

    private void DoConstructor()
    {
        Name = "iconLegend";
        ShowAsModal = false;
    }

    #endregion

    protected override string RenderHtml()
    {
        if (ShowAsModal)
        {
            return GetHtmlModal();
        }

        var field = GetFieldLegend();
        return GetHtmlLegend(field, 0);

    }


    private string GetHtmlLegend(FormElementField field, int identity)
    {
        string tab = "\t".PadRight(identity);

        var html = new StringBuilder();

        html.Append(tab);
        html.AppendLine("<div> ");
        if (field != null)
        {
            var cbo = new JJComboBox(DataAccess);
            cbo.Name = field.Name;
            cbo.DataItem = field.DataItem;
            var list = cbo.GetValues();
            if (list != null && list.Count > 0)
            {
                foreach (DataItemValue item in list)
                { 
                    html.Append(tab);
                    html.Append("\t<div style=\"height:40px\">");

                    var ic = new JJIcon(item.Icon, item.ImageColor, item.Description);
                    ic.CssClass = "fa-fw fa-2x";
                    html.Append(ic.GetHtml());

                    html.Append("&nbsp;&nbsp;");
                    html.Append(Translate.Key(item.Description));
                    html.Append("<br>");
                    html.AppendLine("</div>");
                }
            }
            else
            {
                html.Append(tab);
                html.Append("\t");
                html.Append(Translate.Key("Unable to retrieve caption"));
            }
        }
        else
        {
            html.Append(tab);
            html.Append("\t");
            html.Append(Translate.Key("There is no caption to be displayed"));
        }

        html.Append(tab);
        html.AppendLine("</div>");
        return html.ToString();
    }

    private string GetHtmlModal()
    {
        var field = GetFieldLegend();
        string name = string.Empty;
        if (field != null && !string.IsNullOrEmpty(field.Label))
            name = " - " + field.Label;

        string closeBtn = $"\t\t\t\t\t<button type=\"button\" class=\"{BootstrapHelper.Close}\" {BootstrapHelper.DataDismiss}=\"modal\">{BootstrapHelper.CloseButtonTimes}</button> ";
        int bootstrapVersion = BootstrapHelper.Version;

        StringBuilder html = new();
        html.AppendLine("\t<!-- Start Modal Settings -->");
        html.AppendLine("\t<div class=\"modal\" id=\"" + Name + "\" role=\"dialog\"> ");
        html.AppendLine("\t\t<div class=\"modal-dialog modal-sm\"> ");
        html.AppendLine("\t\t\t<div class=\"modal-content\"> ");
        html.AppendLine("\t\t\t\t<div class=\"modal-header\"> ");
        html.Append("\t\t\t\t\t<h4 class=\"modal-title\">");
        html.Append(Translate.Key("Information"));
        html.Append(" ");
        html.Append(name);
        if(bootstrapVersion < 5 )
            html.Append(closeBtn);
        html.AppendLine("</h4> ");
        if (bootstrapVersion >= 5)
            html.Append(closeBtn);
        html.AppendLine();
        html.AppendLine("\t\t\t\t</div> ");
        html.AppendLine("\t\t\t\t<div class=\"modal-body\"> ");
        html.AppendLine(" ");
        
        html.AppendLine("\t\t\t\t\t<div class=\"form-horizontal\" role=\"form\"> ");
        html.AppendLine(GetHtmlLegend(field, 6));
        html.AppendLine("\t\t\t\t\t</div> ");
        
        html.AppendLine("\t\t\t\t</div> ");

        html.AppendLine("\t\t\t\t<div class=\"modal-footer\"> ");
        html.AppendLine($"\t\t\t\t\t<button type=\"submit\" class=\"{BootstrapHelper.DefaultButton}\" {BootstrapHelper.DataDismiss}=\"modal\">");
        html.AppendLine("\t\t\t\t\t\t<span class=\"fa fa-check\"></span> ");
        html.Append("\t\t\t\t\t\t<span>&nbsp;");
        html.Append(Translate.Key("Ok"));
        html.AppendLine("</span> ");
        html.AppendLine("\t\t\t\t\t</button> ");
        html.AppendLine("\t\t\t\t</div> ");

        html.AppendLine("\t\t\t</div> ");
        html.AppendLine("\t\t</div> ");
        html.AppendLine("\t</div>");
        html.AppendLine("\t<!-- End Modal Settings -->");
        html.AppendLine("");
        return html.ToString();
    }


    private FormElementField GetFieldLegend()
    {
        return FormElement.Fields.FirstOrDefault(f 
            => f.Component == FormComponent.ComboBox && f.DataItem.ShowImageLegend);
    }

}
