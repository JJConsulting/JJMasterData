using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System;
using System.Collections.Generic;
using System.Text;

namespace JJMasterData.Core.WebComponents;

internal class DataImpHelp 
{
    public JJDataImp DataImp { get; private set; }
    
    internal DataImpHelp(JJDataImp dataImp)
    {
        DataImp = dataImp;
    }

    public HtmlElement GetHtmlHelp()
    {
        var panel = new JJCollapsePanel();
        panel.Title = "Import File - Help";
        panel.TitleIcon = new JJIcon(IconType.QuestionCircle);
        panel.ExpandedByDefault = true;
        panel.HtmlElementContent = GetHtmlContent();

        var html = panel.GetHtmlElement()
           .AppendHiddenInput("current_uploadaction", "")
           .AppendHiddenInput("filename", "")
           .AppendElement(GetBackButton().GetHtmlElement());

        return html;
    }

    private HtmlElement GetHtmlContent()
    {
        var list = GetListImportedField();
        var html = new HtmlElement(HtmlTag.Div)
            .AppendElement(HtmlTag.Div, row =>
            {
                row.WithCssClass("row")
                   .AppendElement(HtmlTag.Div, col =>
                    {
                        col.WithCssClass("col-sm-12")
                           .AppendText(GetInfoText(list.Count))
                           .AppendElement(HtmlTag.Br)
                           .AppendElement(HtmlTag.Br);
                    });
            })
            .AppendElement(HtmlTag.Div, div =>
             {
                 div.WithCssClass("table-responsive")
                    .AppendElement(HtmlTag.Table, table =>
                    {
                        table.WithCssClass("table table-hover")
                             .AppendElement(GetHeaderColums())
                             .AppendElement(GetBodyColums(list));
                    });
             });

        return html;
    }

    private HtmlElement GetHeaderColums()
    {
        var head = new HtmlElement(HtmlTag.Thead)
            .AppendElement(HtmlTag.Tr, tr =>
            {
                tr.AppendElement(HtmlTag.Th, th =>
                {
                    th.WithAttribute("style", "width:60px")
                    .AppendText(Translate.Key("Order"));
                });
                tr.AppendElement(HtmlTag.Th, th =>
                {
                    th.AppendText(Translate.Key("Name"));
                });
                tr.AppendElement(HtmlTag.Th, th =>
                {
                    th.WithAttribute("style", "width:120px")
                        .AppendText(Translate.Key("Type"));
                });
                tr.AppendElement(HtmlTag.Th, th =>
                {
                    th.WithAttribute("style", "width:90px")
                        .AppendText(Translate.Key("Required"));
                });
                tr.AppendElement(HtmlTag.Th, th =>
                {
                    th.AppendText(Translate.Key("Details"));
                });
            });

        return head;
    }

    private HtmlElement GetBodyColums(List<FormElementField> list)
    {
        var body = new HtmlElement(HtmlTag.Tbody);
        int orderField = 1;
        foreach (FormElementField field in list)
        {
            body.AppendElement(HtmlTag.Tr, tr =>
            {
                tr.AppendElement(HtmlTag.Td, td =>
                {
                    td.AppendText(orderField.ToString());
                });
                tr.AppendElement(HtmlTag.Td, td =>
                {
                    td.AppendText(string.IsNullOrEmpty(field.Label) ? field.Name : field.Label);
                    td.AppendElementIf(field.IsPk, HtmlTag.Span, span =>
                    {
                        span.WithCssClass("fa fa-star")
                            .WithToolTip(Translate.Key("Primary Key"))
                            .WithAttribute("style", "color:#efd829;");
                    });
                });
                tr.AppendElement(HtmlTag.Td, td =>
                {
                    td.AppendText(GetDataTypeDescription(field.DataType));
                });
                tr.AppendElement(HtmlTag.Td, td =>
                {
                    td.AppendText(field.IsRequired ? Translate.Key("Yes") : Translate.Key("No"));
                });
                tr.AppendElement(HtmlTag.Td, td =>
                {
                    td.AppendText(GetFormatDescription(field));
                });
            });
            orderField++;
        }

        return body;
    }

    private string GetDataTypeDescription(FieldType type)
    {
        switch (type)
        {
            case FieldType.Date:
                return Translate.Key("Date");
            case FieldType.DateTime:
                return Translate.Key("Date and time");
            case FieldType.Int:
                return Translate.Key("Integer number");
            case FieldType.Float:
                return Translate.Key("Decimal number");
            default:
                return Translate.Key("Text");

        }
    }

    private string GetFormatDescription(FormElementField field)
    {
        var html = new StringBuilder();
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

        return html.ToString();
    }

    private string GetInfoText(int columnsCount)
    {
        var upload = DataImp.Upload;
        var text = new StringBuilder();
        text.Append(Translate.Key("To bulk insert records, select a file of type"));
        text.Append("<b>");
        text.Append(upload.AllowedTypes.Replace(",", string.Format(" {0} ", Translate.Key("or"))));
        text.Append("</b>");
        text.Append(", ");
        text.Append(Translate.Key("with the maximum size of"));
        text.Append(" <b>");
        text.Append(Format.FormatFileSize(upload.GetMaxRequestLength()));
        text.Append("</b>");
        text.Append(", ");
        text.Append(Translate.Key("do not include caption or description in the first line"));
        text.Append(", ");
        text.Append(Translate.Key("the file must contain"));
        text.Append(" <b>");
        text.Append(columnsCount);
        text.Append(" ");
        text.Append(Translate.Key("Columns"));
        text.Append(" </b> ");
        text.Append(Translate.Key("separated by semicolons (;), following the layout below:"));

        return text.ToString();
    }

    private string GetHtmlComboHelp(FormElementField f)
    {
        var defaultValues = DataImp.FormManager.GetDefaultValues(null, PageState.Import);
        var cbo = JJComboBox.GetInstance(f, PageState.Import, null, defaultValues, true, null);
        cbo.DataAccess = DataImp.DataAccess;
        cbo.UserValues = DataImp.UserValues;
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

    private JJLinkButton GetBackButton()
    {
        var btnBack = new JJLinkButton();
        btnBack.IconClass = "fa fa-arrow-left";
        btnBack.Text = "Back";
        btnBack.ShowAsButton = true;
        btnBack.OnClientClick = "$('#current_uploadaction').val(''); $('form:first').submit();";

        return btnBack;
    }

    private List<FormElementField> GetListImportedField()
    {
        if (DataImp.FormElement == null)
            throw new ArgumentException(nameof(FormElement));

        var list = new List<FormElementField>();
        foreach (var field in DataImp.FormElement.Fields)
        {
            bool visible = DataImp.FieldManager.IsVisible(field, PageState.Import, null);
            if (visible && field.DataBehavior == FieldBehavior.Real)
                list.Add(field);
        }
        return list;
    }

}
