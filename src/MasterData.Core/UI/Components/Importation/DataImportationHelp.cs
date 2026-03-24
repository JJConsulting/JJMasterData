using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Extensions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class DataImportationHelp
{
    private JJDataImportation DataImportation { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }

    internal DataImportationHelp(JJDataImportation dataImportation)
    {
        DataImportation = dataImportation;
        StringLocalizer = DataImportation.StringLocalizer;
    }

    public async Task<HtmlBuilder> GetHtmlHelpAsync()
    {
        var panel = new JJCollapsePanel
        {
            Title = StringLocalizer["Help"],
            TitleIcon = new JJIcon(FontAwesomeIcon.QuestionCircle),
            ExpandedByDefault = true,
            Content = await GetHtmlContent()
        };

        var html = panel.GetHtmlBuilder()
            .AppendHiddenInput("filename", "");
            if (!string.IsNullOrWhiteSpace(DataImportation.ImportAction.HelpText))
            {
                html.AppendComponent(new JJAlert 
                {
                    Title = StringLocalizer["Information"],
                    Icon = FontAwesomeIcon.InfoCircle,
                    Color = BootstrapColor.Info,
                    Content = new HtmlBuilder(DataImportation.ImportAction.HelpText?.Replace(Environment.NewLine, "<br>") ?? string.Empty, encode:false)
                });
            }
            html.AppendComponent(GetBackButton())
            .AppendDiv(div =>
            {
                div.WithCssClass(BootstrapHelper.PullRight);
                div.AppendComponent(DataImportation.CloseButton);
            });
        return html;
    }

    private async Task<HtmlBuilder> GetHtmlContent()
    {
        var list = GetImportedFieldList();
        var html = new HtmlBuilder(HtmlTag.Div)
            .Append(HtmlTag.Div, row =>
            {
                row.WithCssClass("row")
                    .Append(HtmlTag.Div, col =>
                    {
                        col.WithCssClass("col-sm-12")
                            .Append(GetInfoHtml(list.Count))
                            .Append(HtmlTag.Br)
                            .Append(HtmlTag.Br);
                    });
            });

        var bodyHtml = await GetBodyHtml(list);
        html.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("table-responsive");
            div.Append(HtmlTag.Table, table =>
            {
                table.WithCssClass("table table-hover")
                    .Append(GetHeaderColumns())
                    .Append(bodyHtml);
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
                    th.WithStyle( "width:60px")
                        .AppendText(StringLocalizer["Order"]);
                });
                tr.Append(HtmlTag.Th, th => th.AppendText(StringLocalizer["Name"]));
                tr.Append(HtmlTag.Th, th =>
                {
                    th.WithStyle( "width:120px")
                        .AppendText(StringLocalizer["Type"]);
                });
                tr.Append(HtmlTag.Th, th =>
                {
                    th.WithStyle( "width:90px")
                        .AppendText(StringLocalizer["Required"]);
                });
                tr.Append(HtmlTag.Th, th => th.AppendText(StringLocalizer["Details"]));
            });

        return head;
    }

    private async Task<HtmlBuilder> GetBodyHtml(List<FormElementField> list)
    {
        var body = new HtmlBuilder(HtmlTag.Tbody);
        var orderField = 1;
        foreach (var field in list)
        {
            var tr = new HtmlBuilder(HtmlTag.Tr);
            var currentOrderField = orderField;
            tr.Append(HtmlTag.Td, td => td.AppendText(currentOrderField.ToString()));
            tr.Append(HtmlTag.Td, td =>
            {
                td.AppendText(field.LabelOrName);
                td.AppendIf(field.IsPk, HtmlTag.Span, span =>
                {
                    span.WithCssClass("fa fa-star")
                        .WithToolTip(StringLocalizer["Primary Key"])
                        .WithStyle( "color:#efd829;");
                });
            });
            tr.Append(HtmlTag.Td, td => td.AppendText(GetDataTypeDescription(field.DataType)));
            tr.Append(HtmlTag.Td,
                td => td.AppendText(field.IsRequired ? StringLocalizer["Yes"] : StringLocalizer["No"]));

            var formatDescription = await GetFormatDescriptionHtml(field);
            
            tr.Append(HtmlTag.Td, td => td.Append(formatDescription));

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
            case FieldType.Decimal:
                return StringLocalizer["Decimal number"];
            default:
                return StringLocalizer["Text"];
        }
    }

    private async Task<HtmlBuilder> GetFormatDescriptionHtml(FormElementField field)
    {
        var text = new HtmlBuilder();
        switch (field.Component)
        {
            case FormComponent.Date:
                text.AppendText(StringLocalizer[$"Format ({Format.DateFormat}) example:"]);
                text.AppendText(' ');
                text.AppendText(DateTime.Now.ToString($"{Format.DateFormat}"));
                text.AppendText('.');
                break;
            case FormComponent.DateTime:
                text.AppendText(StringLocalizer[$"Format ({Format.DateTimeFormat}) example:"]);
                text.AppendText(' ');
                text.AppendText(DateTime.Now.ToString($"{Format.DateTimeFormat}"));
                text.AppendText('.');
                break;
            case FormComponent.ComboBox or FormComponent.RadioButtonGroup:
                text.AppendText(StringLocalizer["Inform the Id"]);
                text.AppendText(' ');
                text.Append(await GetHtmlComboHelp(field));
                break;
            case FormComponent.CheckBox:
                text.AppendText("(1, 0).");
                break;
            default:
            {
                if (field.DataType == FieldType.Int)
                {
                    text.AppendText(StringLocalizer["No dot or comma."]);
                }
                else if (field.DataType is FieldType.Float or FieldType.Decimal)
                {
                    if (field.Size > 0)
                    {
                        text.AppendText(DataImportation.StringLocalizer["Max. {0} characters.", field.Size]);
                    }

                    text.AppendText(DataImportation.StringLocalizer["Use '{0}' as separator for {1} decimal places.",
                        CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator, field.NumberOfDecimalPlaces]);
                }
                else
                {
                    if (field.Size > 0)
                    {
                        text.AppendText(DataImportation.StringLocalizer["Max. {0} characters.", field.Size]);
                    }
                }

                break;
            }
        }

        if (!string.IsNullOrEmpty(field.HelpDescription))
        {
            text.AppendBr();
            text.AppendText(field.HelpDescription);
        }

        return text;
    }

    private HtmlBuilder GetInfoHtml(int columnsCount)
    {
        var upload = DataImportation.UploadArea;
        var infoHtml = new HtmlBuilder();

        infoHtml.AppendText(StringLocalizer["To bulk insert records, select a file of type"]);

        infoHtml.AppendB(b =>
        {
            b.AppendText(upload.AllowedTypes.Replace(",", $" {StringLocalizer["or"]} "));
        });

        infoHtml.AppendText(", ");
        infoHtml.AppendText(StringLocalizer["with the maximum size of"]);

        infoHtml.AppendText(" ");
        infoHtml.AppendB(b =>
        {
            b.AppendText(Format.FormatFileSize(upload.GetMaxRequestLength()));
        });

        infoHtml.AppendText(", ");
        infoHtml.AppendText(StringLocalizer["do not include caption or description in the first line"]);
        infoHtml.AppendText(", ");
        infoHtml.AppendText(StringLocalizer["the file must contain"]);

        infoHtml.AppendText(" ");
        infoHtml.AppendB(b =>
        {
            b.AppendText(columnsCount.ToString());
            b.AppendText(" ");
            b.AppendText(StringLocalizer["Columns"]);
        });

        infoHtml.AppendText(" ");
        infoHtml.AppendText(StringLocalizer["separated by semicolons (;), following the layout below:"]);

        return infoHtml;
    }

    private async Task<HtmlBuilder> GetHtmlComboHelp(FormElementField field)
    {
        var defaultValues = await DataImportation.FieldValuesService.GetDefaultValuesAsync(DataImportation.FormElement,
            new FormStateData(new Dictionary<string, object>(), DataImportation.UserValues, PageState.Import));
        var formStateData = new FormStateData(defaultValues, DataImportation.UserValues, PageState.Import);
        var dataQuery = new DataQuery(formStateData, DataImportation.FormElement.ConnectionId);
        var items = await DataImportation.DataItemService.GetValuesAsync(field.DataItem!, dataQuery);

        if (items.Count == 0)
            return new();

        var isFirst = true;

        var span = new HtmlBuilder(HtmlTag.Span);
        span.WithCssClass("small");
        span.Append(HtmlTag.Span, span =>
        {
            span.AppendText("(");

            foreach (var item in items)
            {
                if (isFirst)
                    isFirst = false;
                else
                    span.AppendText(", ");

                span.Append(HtmlTag.B, b => b.AppendText(item.Id));

                span.AppendText("=");
                span.AppendText(item.Description?.Trim() ?? string.Empty);
            }

            span.AppendText(").");

            if (field.DataItem!.EnableMultiSelect)
            {
                span.AppendText(
                    $" {StringLocalizer["To select more than one item, enter the desired values separated by a comma."]}");
            }
        });


        return span;
    }

    private JJLinkButton GetBackButton()
    {
        var btnBack = new JJLinkButton
        {
            IconClass = "fa fa-arrow-left",
            Text = StringLocalizer["Back"],
            ShowAsButton = true,
            OnClientClick = DataImportation.DataImportationScripts.GetBackScript()
        };

        return btnBack;
    }

    private List<FormElementField> GetImportedFieldList()
    {
        if (DataImportation.FormElement == null)
            throw new ArgumentException(nameof(FormElement));

        var defaultValues = new Dictionary<string, object>();
        var formData = new FormStateData(defaultValues, PageState.Import);
        var list = new List<FormElementField>();
        foreach (var field in DataImportation.FormElement.Fields)
        {
            bool visible = DataImportation.ExpressionsService.GetBoolValue(field.VisibleExpression, formData);
            if (visible && field.DataBehavior is FieldBehavior.Real or FieldBehavior.WriteOnly)
                list.Add(field);
        }

        return list;
    }
}