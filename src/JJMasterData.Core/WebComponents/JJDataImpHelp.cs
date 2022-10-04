using System;
using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.WebComponents;

public class JJDataImpHelp : JJBaseView
{
 
    #region "Properties"

    private FieldManager _fieldManager;
    private FormManager _formManager;
    private JJUploadFile _upload;
    
    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }


    /// <summary>
    /// Objeto responsável por realizar upload dos arquivos
    /// </summary>
    public JJUploadFile Upload
    {
        get => _upload ??= new JJUploadFile
        {
            Multiple = false,
            EnableCopyPaste = false,
            Name = Name + "_upload",
            AllowedTypes = "txt,csv,log"
        };
        set => _upload = value;
    }

    

    /// <summary>
    /// Funções úteis para manipular campos no formulário
    /// </summary>
    private FieldManager FieldManager => _fieldManager ??= new FieldManager(this, FormElement);

    internal FormManager FormManager
    {
        get => _formManager ??= new FormManager(FormElement, UserValues, DataAccess);
        private set => _formManager = value;
    }


    #endregion

    #region "Constructors"

    internal JJDataImpHelp(JJDataImp baseView)
    {
        FormElement = baseView.FormElement;
        FormManager = baseView.FormManager;
        Upload = baseView.Upload;
        UserValues = baseView.UserValues;
        DataAccess = baseView.DataAccess;
        Name = baseView.Name;
    }

    public JJDataImpHelp(FormElement formElement, IDataAccess dataAccess) 
    {
        FormElement = formElement;
        DataAccess = dataAccess;
        Name = "jjdataimp1";
    }

    #endregion

    protected override string RenderHtml()
    {
        return GetHtmlHelp();
    }

    /// <summary>
    /// Recupera o html de ajuda
    /// </summary>
    private string GetHtmlHelp()
    {
        var list = GetListImportedField();
        StringBuilder html = new StringBuilder();

        html.AppendLine("<input type=\"hidden\" id=\"current_uploadaction\" name=\"current_uploadaction\" value=\"\" />");
        html.AppendLine("<input type=\"hidden\" id=\"filename\" name=\"filename\" />");
        html.AppendLine("");
        html.AppendLine($"<div class=\"{BootstrapHelper.PanelGroup}\" id=\"divNovo\" runat=\"server\" enableviewstate=\"false\">");
        html.AppendLine($"\t<div class=\"{BootstrapHelper.GetPanel("default")}\">");
        html.Append($"\t\t<div class=\"{BootstrapHelper.GetPanelHeading("default")}\" href=\"#collapse1\" {BootstrapHelper.DataToggle}=\"collapse\" data-target=\"#collapse1\" aria-expanded=\"true\">");
        html.AppendLine($"\t\t\t<h4 class=\"{BootstrapHelper.PanelTitle}\">");

        html.Append('\t', 4);
        html.Append("<a>");
        html.Append("<span class=\"fa fa-question-circle\"></span> ");
        html.Append(Translate.Key("Import File - Help"));
        html.AppendLine(" </a>");

        html.AppendLine("\t\t\t</h4>");
        html.AppendLine("\t\t</div>");
        html.Append($"\t\t<div id=\"collapse1\" class=\"{BootstrapHelper.PanelCollapse}\">");
        html.AppendLine($"\t\t\t<div class=\"{BootstrapHelper.PanelBody}\">");

        html.AppendLine("\t<div class=\"row\">");
        html.AppendLine("\t\t<div class=\"col-sm-12\">");
        html.Append("\t\t\t");
        html.Append(Translate.Key("To bulk insert records, select a file of type"));
        html.Append(" <b>");
        string separeteString = string.Format(" {0} ", Translate.Key("or"));
        html.Append(Upload.AllowedTypes.Replace(",", separeteString));
        html.Append("</b>");
        html.Append(", ");
        html.Append(Translate.Key("with the maximum size of"));
        html.Append(" <b>");
        html.Append(Format.FormatFileSize(Upload.GetMaxRequestLength()));
        html.Append("</b>");
        html.Append(", ");
        html.Append(Translate.Key("do not include caption or description in the first line"));
        html.Append(", ");
        html.Append(Translate.Key("the file must contain"));
        html.Append(" <b>");
        html.Append(list.Count);
        html.Append(" ");
        html.Append(Translate.Key("Columns"));
        html.Append(" </b> ");
        html.Append(Translate.Key("separated by semicolons (;), following the layout below:"));
        html.AppendLine("<br><br>");
        html.AppendLine("\t\t</div>");
        html.AppendLine("\t</div>");

        html.AppendLine("\t<div class=\"table-responsive\">");
        html.AppendLine("\t\t<table class=\"table table-hover\">");
        html.AppendLine("\t\t\t<thead>");
        html.AppendLine("\t\t\t\t<tr>");

        html.Append('\t', 5);
        html.Append("<th style=\"width:60px\">");
        html.Append(Translate.Key("Order"));
        html.AppendLine("</th>");

        html.Append('\t', 5);
        html.Append("<th>");
        html.Append(Translate.Key("Name"));
        html.AppendLine("</th>");

        html.Append('\t', 5);
        html.Append("<th style=\"width:120px\">");
        html.Append(Translate.Key("Type"));
        html.AppendLine("</th>");

        html.Append('\t', 5);
        html.Append("<th style=\"width:90px\">");
        html.Append(Translate.Key("Required"));
        html.AppendLine("</th>");

        html.Append('\t', 5);
        html.Append("<th>");
        html.Append(Translate.Key("Details"));
        html.AppendLine("</th>");

        html.AppendLine("\t\t\t\t</tr>");
        html.AppendLine("\t\t\t</thead>");
        html.AppendLine("\t\t\t<tbody>");
        int orderField = 1;
        foreach (FormElementField field in list)
        {
            html.AppendLine("\t\t\t\t<tr>");
            html.Append("\t\t\t\t\t<td>");
            html.Append(orderField);
            html.AppendLine("\t\t\t\t\t</td>");
            html.Append("\t\t\t\t\t<td>");
            html.Append(string.IsNullOrEmpty(field.Label) ? field.Name : field.Label);
            if (field.IsPk)
            {
                html.Append(" <span class='fa fa-star' ");
                html.Append(" style='color:#efd829;' ");
                html.Append($" {BootstrapHelper.DataToggle}='tooltip' ");
                html.AppendFormat(" title='{0}'></span>", Translate.Key("Primary Key"));
            }
            html.AppendLine("</td>");
            html.Append("\t\t\t\t\t<td>");
            if (field.DataType == FieldType.Date)
            {
                html.Append(Translate.Key("Date"));
            }
            else if (field.DataType == FieldType.DateTime)
            {
                html.Append(Translate.Key("Date and time"));
            }
            else if (field.DataType == FieldType.Int)
            {
                html.Append(Translate.Key("Integer number"));
            }
            else if (field.DataType == FieldType.Float)
            {
                html.Append(Translate.Key("Decimal number"));
            }
            else
            {
                html.Append(Translate.Key("Text"));
            }
            html.AppendLine("</td>");
            html.Append("\t\t\t\t\t<td>");
            if (field.IsRequired)
                html.Append(Translate.Key("Yes"));
            else
                html.Append(Translate.Key("No"));
            html.AppendLine("</td>");
            html.Append("\t\t\t\t\t<td>");
            if (field.Component == FormComponent.Date)
            {
                html.Append(Translate.Key($"Format ({Format.DateFormat}) example:"));
                html.Append(" ");
                html.Append(DateTime.Now.ToString($"{Format.DateFormat}"));
            }
            else if (field.Component == FormComponent.DateTime)
            {
                html.Append(Translate.Key($"Format ({Format.DateTimeFormat}) example:"));
                html.Append(" ");
                html.Append(DateTime.Now.ToString($"{Format.DateTimeFormat}"));
            }
            else if (field.Component == FormComponent.ComboBox)
            {
                html.Append(Translate.Key("Inform the Id"));
                html.Append(" ");
                html.Append(GetHtmlComboHelp(field));
            }
            else if (field.Component == FormComponent.CheckBox)
            {
                html.Append("(1,S,Y) ");
                html.Append(Translate.Key("Selected"));
            }
            else if (field.DataType == FieldType.Int)
            {
                html.Append(Translate.Key("No dot or comma"));
            }
            else if (field.DataType == FieldType.Float)
            {
                if (field.Size > 0)
                {
                    html.Append(Translate.Key("Max. {0} characters.", field.Size));
                }

                html.Append(Translate.Key("Use comma as separator for {0} decimal places", field.NumberOfDecimalPlaces));
            }
            else
            {
                if (field.Size > 0)
                {
                    html.Append(Translate.Key("Max. {0} characters.", field.Size));
                }
            }
            
            if (!string.IsNullOrEmpty(field.HelpDescription))
            {
                html.Append("<br>");
                html.Append(field.HelpDescription);
            }

            html.AppendLine("</td>");
            html.AppendLine("\t\t\t\t</tr>");
            orderField++;
        }
        html.AppendLine("\t\t\t</tbody>");
        html.AppendLine("\t\t</table>");

        html.AppendLine("\t</div>");
        html.AppendLine("\t\t\t</div>");
        html.AppendLine("\t\t</div>");
        html.AppendLine("\t</div>");
        html.AppendLine("</div>");

        html.AppendLine("<div class=\"row\">");
        html.AppendLine("\t<div class=\"col-sm-12\">");

        var btnBack = new JJLinkButton();
        btnBack.IconClass = "fa fa-arrow-left";
        btnBack.Text = "Back";
        btnBack.ShowAsButton = true;
        btnBack.OnClientClick = "$('#current_uploadaction').val(''); $('form:first').submit();";
        html.AppendLine(btnBack.GetHtml());

        html.AppendLine("\t</div>");
        html.AppendLine("</div>");

        return html.ToString();
    }

    /// <summary>
    /// Recupera um informativo dos ids validos de uma combo
    /// </summary>
    private string GetHtmlComboHelp(FormElementField f)
    {
        var defaultValues = FormManager.GetDefaultValues(null, PageState.Import);
        var cbo = JJComboBox.GetInstance(f, PageState.Import, null, defaultValues, true, null);
        cbo.DataAccess = DataAccess;
        cbo.UserValues = UserValues;
        cbo.Name = f.Name;
        cbo.Visible = true;
        cbo.DataItem = f.DataItem;

        var itens = cbo.GetValues();
        if (itens.Count == 0)
            return string.Empty;

        bool isFirst = true;
        var sValues = new StringBuilder();
        sValues.Append("<span class=\"small\">(");
        foreach (DataItemValue item in itens)
        {
            if (isFirst)
                isFirst = false;
            else
                sValues.Append(", ");

            sValues.Append("<b>");
            sValues.Append(item.Id);
            sValues.Append("</b>");
            sValues.Append("=");
            sValues.Append(item.Description.Trim());
        }
        sValues.Append(")</span>");

        return sValues.ToString();
    }

    /// <summary>
    /// Lista de campos a serem importados
    /// </summary>
    private List<FormElementField> GetListImportedField()
    {
        if (FormElement == null)
            throw new ArgumentException(nameof(FormElement));

        var list = new List<FormElementField>();
        foreach (var field in FormElement.Fields)
        {
            bool visible = FieldManager.IsVisible(field, PageState.Import, null);
            if (visible && field.DataBehavior == FieldBehavior.Real)
                list.Add(field);
        }
        return list;
    }

    


}
