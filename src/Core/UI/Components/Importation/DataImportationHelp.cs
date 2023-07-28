using System;
using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

internal class DataImportationHelp
{
    public JJDataImp DataImportation { get; private set; }
    public IStringLocalizer<JJMasterDataResources> StringLocalizer { get; private set; }
    internal DataImportationHelp(JJDataImp dataImportation)
    {
        DataImportation = dataImportation;
        StringLocalizer = DataImportation.StringLocalizer;
    }

    public HtmlBuilder GetHtmlHelp()
    {
        var panel = new JJCollapsePanel(DataImportation.CurrentContext)
        {
            Title = "Import File - Help",
            TitleIcon = new JJIcon(IconType.QuestionCircle),
            ExpandedByDefault = true,
            HtmlBuilderContent = GetHtmlContent()
        };

        var html = panel.RenderHtml()
           .AppendHiddenInput("current_uploadaction", "")
           .AppendHiddenInput("filename", "")
           .AppendElement(GetBackButton());

        return html;
    }

    private HtmlBuilder GetHtmlContent()
    {
        var list = GetListImportedField();
        var html = new HtmlBuilder(HtmlTag.Div)
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
                             .AppendElement(GetHeaderColumns())
                             .AppendElement(GetBodyColums(list));
                    });
             });

        return html;
    }

    private HtmlBuilder GetHeaderColumns()
    {
        var head = new HtmlBuilder(HtmlTag.Thead)
            .AppendElement(HtmlTag.Tr, tr =>
            {
                tr.AppendElement(HtmlTag.Th, th =>
                {
                    th.WithAttribute("style", "width:60px")
                    .AppendText(StringLocalizer["Order"]);
                });
                tr.AppendElement(HtmlTag.Th, th =>
                {
                    th.AppendText(StringLocalizer["Name"]);
                });
                tr.AppendElement(HtmlTag.Th, th =>
                {
                    th.WithAttribute("style", "width:120px")
                        .AppendText(StringLocalizer["Type"]);
                });
                tr.AppendElement(HtmlTag.Th, th =>
                {
                    th.WithAttribute("style", "width:90px")
                        .AppendText(StringLocalizer["Required"]);
                });
                tr.AppendElement(HtmlTag.Th, th =>
                {
                    th.AppendText(StringLocalizer["Details"]);
                });
            });

        return head;
    }

    private HtmlBuilder GetBodyColums(List<FormElementField> list)
    {
        var body = new HtmlBuilder(HtmlTag.Tbody);
        int orderField = 1;
        foreach (FormElementField field in list)
        {
            var tr = new HtmlBuilder(HtmlTag.Tr);
            var currentOrderField = orderField;
            tr.AppendElement(HtmlTag.Td, td =>
            {
                td.AppendText(currentOrderField.ToString());
            });
            tr.AppendElement(HtmlTag.Td, td =>
            {
                td.AppendText(string.IsNullOrEmpty(field.Label) ? field.Name : field.Label);
                td.AppendElementIf(field.IsPk, HtmlTag.Span, span =>
                {
                    span.WithCssClass("fa fa-star")
                        .WithToolTip(StringLocalizer["Primary Key"])
                        .WithAttribute("style", "color:#efd829;");
                });
            });
            tr.AppendElement(HtmlTag.Td, td =>
            {
                td.AppendText(GetDataTypeDescription(field.DataType));
            });
            tr.AppendElement(HtmlTag.Td, td =>
            {
                td.AppendText(field.IsRequired ? StringLocalizer["Yes"] : StringLocalizer["No"]);
            });
            tr.AppendElement(HtmlTag.Td, td =>
            {
                td.AppendText(GetFormatDescription(field));
            });

            body.AppendElement(tr);
            orderField++;
        }

        return body;
    }

    private string GetDataTypeDescription(FieldType type)
    {
        switch (type)
        {
            case FieldType.Date:
                return StringLocalizer["Date"];
            case FieldType.DateTime:
            case FieldType.DateTime2:
                return StringLocalizer["Date and time"];
            case FieldType.Int:
                return StringLocalizer["Integer number"];
            case FieldType.Float:
                return StringLocalizer["Decimal number"];
            default:
                return StringLocalizer["Text"];

        }
    }

    private string GetFormatDescription(FormElementField field)
    {
        var text = new StringBuilder();
        if (field.Component == FormComponent.Date)
        {
            text.Append(StringLocalizer[$"Format ({Format.DateFormat}) example:"]);
            text.Append(" ");
            text.Append(DateTime.Now.ToString($"{Format.DateFormat}"));
        }
        else if (field.Component == FormComponent.DateTime)
        {
            text.Append(StringLocalizer[$"Format ({Format.DateTimeFormat}) example:"]);
            text.Append(" ");
            text.Append(DateTime.Now.ToString($"{Format.DateTimeFormat}"));
        }
        else if (field.Component == FormComponent.ComboBox)
        {
            text.Append(StringLocalizer["Inform the Id"]);
            text.Append(" ");
            text.Append(GetHtmlComboHelp(field));
        }
        else if (field.Component == FormComponent.CheckBox)
        {
            text.Append("(1,S,Y) ");
            text.Append(StringLocalizer["Selected"]);
        }
        else if (field.DataType == FieldType.Int)
        {
            text.Append(StringLocalizer["No dot or comma"]);
        }
        else if (field.DataType == FieldType.Float)
        {
            if (field.Size > 0)
            {
                text.Append(DataImportation.StringLocalizer["Max. {0} characters.", field.Size]);
            }

            text.Append(DataImportation.StringLocalizer["Use comma as separator for {0} decimal places", field.NumberOfDecimalPlaces]);
        }
        else
        {
            if (field.Size > 0)
            {
                text.Append(DataImportation.StringLocalizer["Max. {0} characters.", field.Size]);
            }
        }

        if (!string.IsNullOrEmpty(field.HelpDescription))
        {
            text.Append("<br>");
            text.Append(field.HelpDescription);
        }

        return text.ToString();
    }

    private string GetInfoText(int columnsCount)
    {
        var upload = DataImportation.Upload;
        var text = new StringBuilder();
        text.Append(StringLocalizer["To bulk insert records, select a file of type"]);
        text.Append("<b>");
        text.Append(upload.AllowedTypes.Replace(",", $" {StringLocalizer["or"]} "));
        text.Append("</b>");
        text.Append(", ");
        text.Append(StringLocalizer["with the maximum size of"]);
        text.Append(" <b>");
        text.Append(Format.FormatFileSize(upload.GetMaxRequestLength()));
        text.Append("</b>");
        text.Append(", ");
        text.Append(StringLocalizer["do not include caption or description in the first line"]);
        text.Append(", ");
        text.Append(StringLocalizer["the file must contain"]);
        text.Append(" <b>");
        text.Append(columnsCount);
        text.Append(" ");
        text.Append(StringLocalizer["Columns"]);
        text.Append(" </b> ");
        text.Append(StringLocalizer["separated by semicolons (;), following the layout below:"]);

        return text.ToString();
    }

    private string GetHtmlComboHelp(FormElementField field)
    {
        var defaultValues = DataImportation.FieldValuesService.GetDefaultValues(DataImportation.FormElement,null, PageState.Import);
        var expOptions = new FormStateData(DataImportation.UserValues, defaultValues, PageState.Import);
        var comboBox = DataImportation.ComboBoxFactory.Create(null,field, expOptions, null,null);
        var items = comboBox.GetValues();

        if (items.Count == 0)
            return string.Empty;

        var isFirst = true;

        var span = new HtmlBuilder(HtmlTag.Span);
        span.WithCssClass("small");
        span.AppendElement(HtmlTag.Span, span =>
        {
            span.AppendText("(");

   
            foreach (var item in items)
            {
                if (isFirst)
                    isFirst = false;
                else
                    span.AppendText(", ");

                span.AppendElement(HtmlTag.B, b =>
                {
                    b.AppendText(item.Id);
                });

                span.AppendText("=");
                span.AppendText(item.Description.Trim());
            }

            span.AppendText(").");

            if (field.DataItem!.EnableMultiSelect)
            {
                span.AppendText(" " + StringLocalizer["To select more than one item, enter the desired values separated by a comma."]);
            }
        });


        return span.ToString();
    }

    private JJLinkButton GetBackButton()
    {
        var btnBack = new JJLinkButton
        {
            IconClass = "fa fa-arrow-left",
            Text = "Back",
            ShowAsButton = true,
            OnClientClick = "$('#current_uploadaction').val(''); $('form:first').submit();"
        };

        return btnBack;
    }

    private List<FormElementField> GetListImportedField()
    {
        if (DataImportation.FormElement == null)
            throw new ArgumentException(nameof(FormElement));

        var list = new List<FormElementField>();
        foreach (var field in DataImportation.FormElement.Fields)
        {
            bool visible = DataImportation.FieldVisibilityService.IsVisible(field, PageState.Import, null);
            if (visible && field.DataBehavior == FieldBehavior.Real)
                list.Add(field);
        }
        return list;
    }

}
