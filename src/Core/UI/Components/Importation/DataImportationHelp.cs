using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

internal class DataImportationHelp
{
    public JJDataImportation DataImportation { get; private set; }
    public IStringLocalizer<JJMasterDataResources> StringLocalizer { get; private set; }
    internal DataImportationHelp(JJDataImportation dataImportation)
    {
        DataImportation = dataImportation;
        StringLocalizer = DataImportation.StringLocalizer;
    }

    public async Task<HtmlBuilder> GetHtmlHelpAsync()
    {
        var panel = new JJCollapsePanel(DataImportation.CurrentContext.Request.Form)
        {
            Title = "Import File - Help",
            TitleIcon = new JJIcon(IconType.QuestionCircle),
            ExpandedByDefault = true,
            HtmlBuilderContent = await GetHtmlContent()
        };

        var html = panel.BuildHtml()
           .AppendHiddenInput("filename", "")
           .AppendComponent(GetBackButton());

        return html;
    }

    private async Task<HtmlBuilder> GetHtmlContent()
    {
        var list = await GetListImportedField();
        var html = new HtmlBuilder(HtmlTag.Div)
            .Append(HtmlTag.Div, row =>
            {
                row.WithCssClass("row")
                    .Append(HtmlTag.Div, col =>
                    {
                        col.WithCssClass("col-sm-12")
                            .AppendText(GetInfoText(list.Count))
                            .Append(HtmlTag.Br)
                            .Append(HtmlTag.Br);
                    });
            });
           await html.AppendAsync(HtmlTag.Div, async div =>
             {
                 div.WithCssClass("table-responsive");
                     await div.AppendAsync(HtmlTag.Table, async table =>
                    {
                        table.WithCssClass("table table-hover")
                             .Append(GetHeaderColumns())
                             .Append(await GetBodyColums(list));
                    });
             });

        return html;
    }

    private HtmlBuilder GetHeaderColumns()
    {
        var head = new HtmlBuilder(HtmlTag.Thead)
            .Append(HtmlTag.Tr, tr =>
            {
                tr.Append(HtmlTag.Th, th =>
                {
                    th.WithAttribute("style", "width:60px")
                    .AppendText(StringLocalizer["Order"]);
                });
                tr.Append(HtmlTag.Th, th =>
                {
                    th.AppendText(StringLocalizer["Name"]);
                });
                tr.Append(HtmlTag.Th, th =>
                {
                    th.WithAttribute("style", "width:120px")
                        .AppendText(StringLocalizer["Type"]);
                });
                tr.Append(HtmlTag.Th, th =>
                {
                    th.WithAttribute("style", "width:90px")
                        .AppendText(StringLocalizer["Required"]);
                });
                tr.Append(HtmlTag.Th, th =>
                {
                    th.AppendText(StringLocalizer["Details"]);
                });
            });

        return head;
    }

    private async Task<HtmlBuilder> GetBodyColums(List<FormElementField> list)
    {
        var body = new HtmlBuilder(HtmlTag.Tbody);
        int orderField = 1;
        foreach (var field in list)
        {
            var tr = new HtmlBuilder(HtmlTag.Tr);
            var currentOrderField = orderField;
            tr.Append(HtmlTag.Td, td =>
            {
                td.AppendText(currentOrderField.ToString());
            });
            tr.Append(HtmlTag.Td, td =>
            {
                td.AppendText(field.LabelOrName);
                td.AppendIf(field.IsPk, HtmlTag.Span, span =>
                {
                    span.WithCssClass("fa fa-star")
                        .WithToolTip(StringLocalizer["Primary Key"])
                        .WithAttribute("style", "color:#efd829;");
                });
            });
            tr.Append(HtmlTag.Td, td =>
            {
                td.AppendText(GetDataTypeDescription(field.DataType));
            });
            tr.Append(HtmlTag.Td, td =>
            {
                td.AppendText(field.IsRequired ? StringLocalizer["Yes"] : StringLocalizer["No"]);
            });
            await tr.AppendAsync(HtmlTag.Td, async td =>
            {
                td.AppendText(await GetFormatDescription(field));
            });

            body.Append(tr);
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

    private async Task<string> GetFormatDescription(FormElementField field)
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
            text.Append(await GetHtmlComboHelp(field));
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
        var upload = DataImportation.UploadArea;
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

    private async Task<string> GetHtmlComboHelp(FormElementField field)
    {
        var defaultValues = await DataImportation.FieldsService.GetDefaultValuesAsync(DataImportation.FormElement,null, PageState.Import);
        var expOptions = new FormStateData(defaultValues, DataImportation.UserValues, PageState.Import);
        //TODO: DataItemService is better
        var comboBox = DataImportation.ComponentFactory.Controls.Create<JJComboBox>(null,field, new(expOptions,null));
        var items = await comboBox.GetValuesAsync().ToListAsync();

        if (items.Count == 0)
            return string.Empty;

        var isFirst = true;

        var span = new HtmlBuilder(HtmlTag.Span);
        span.WithCssClass("small");
        span.Append(HtmlTag.Span,  span =>
        {
            span.AppendText("(");
            
            foreach (var item in items)
            {
                if (isFirst)
                    isFirst = false;
                else
                    span.AppendText(", ");

                span.Append(HtmlTag.B, b =>
                {
                    b.AppendText(item.Id);
                });

                span.AppendText("=");
                span.AppendText(item.Description.Trim());
            }

            span.AppendText(").");

            if (field.DataItem!.EnableMultiSelect)
            {
                span.AppendText(
                    $" {StringLocalizer["To select more than one item, enter the desired values separated by a comma."]}");
            }
        });


        return span.ToString();
    }

    private JJLinkButton GetBackButton()
    {
        var btnBack = DataImportation.ComponentFactory.Html.LinkButton.Create();
        btnBack.IconClass = "fa fa-arrow-left";
        btnBack.Text = "Back";
        btnBack.ShowAsButton = true;
        btnBack.OnClientClick = DataImportation.DataImportationScripts.GetShowScript();

        return btnBack;
    }

    private async Task<List<FormElementField>> GetListImportedField()
    {
        if (DataImportation.FormElement == null)
            throw new ArgumentException(nameof(FormElement));

        var defaultValues = new Dictionary<string, object>();
        var formData = new FormStateData(defaultValues, PageState.Import);
        var list = new List<FormElementField>();
        foreach (var field in DataImportation.FormElement.Fields)
        {
            bool visible = await DataImportation.ExpressionsService.GetBoolValueAsync(field.VisibleExpression, formData);
            if (visible && field.DataBehavior == FieldBehavior.Real)
                list.Add(field);
        }
        return list;
    }

}
