using JJMasterData.Web.DataManager.Exportation;
#nullable disable warnings

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Configuration;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Components;

internal sealed class DataExportationSettings(JJDataExportation dataExportation)
{
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer  = dataExportation.StringLocalizer;
    
    internal async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var html = new HtmlBuilder(HtmlTag.Div);
        
        var folderPath = DataExportationHelper.GetExportationFolderPath(
            dataExportation.FormElement,
            dataExportation.MasterDataOptions.ExportationFolderPath,
            dataExportation.UserId);
        
        html.WithCssClass("container-fluid");
        html.Append(await GetFormHtmlElementAsync(folderPath));
        html.AppendHr();
        html.AppendDiv(div =>
        {
            div.WithCssClass("row");
            div.AppendDiv(div =>
            {
                string onClientClick = dataExportation.Scripts.GetStartExportationScript();
                
                var btnOk = new JJLinkButton
                {
                    Text = _stringLocalizer["Export"],
                    IconClass = "fa fa-check",
                    ShowAsButton = true,
                    OnClientClick = onClientClick
                };

                var btnCancel = new JJLinkButton
                {
                    Text = _stringLocalizer["Cancel"],
                    IconClass = "fa fa-times",
                    ShowAsButton = true
                };

                btnCancel.Attributes.Add(BootstrapHelper.DataDismiss, "modal");

                div.WithCssClass($"col-sm-12 {BootstrapHelper.TextRight}");
                div.AppendComponent(btnOk);
                div.AppendText(" ");
                div.AppendComponent(btnCancel);
            });
        });

        return html;
    }

    private async Task<HtmlBuilder> GetFormHtmlElementAsync(string exportationFolderPath)
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass("row");

        div.Append(GetFileExtensionField());

        div.Append(GetOrientationField());

        div.Append(GetExportAllField());

        div.Append(GetDelimiterField());

        div.Append(GetFirstLineField());

        div.Append(await GetFilesCollapsePanelHtmlBuilderAsync(exportationFolderPath));

        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("row");
            div.Append(HtmlTag.Label, label =>
            {
                label.WithCssClass("small col-sm-12");
                label.Append(HtmlTag.Br);
                label.AppendComponent(new JJIcon
                {
                    IconClass = "text-info fa fa-info-circle"
                });
                label.AppendText(
                    $" {_stringLocalizer["Filters performed in the previous screen will be considered in the export"]}");
            });
        });

        div.AppendComponent(GetTooManyRecordsAlert(dataExportation.Name));

        return div;
    }

    private HtmlBuilder GetFileExtensionField()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("col-sm-4")
            .WithCssClass(BootstrapHelper.FormGroup)
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", $"{dataExportation.Name}{ExportOptions.FileName}");
                label.AppendText(_stringLocalizer["Export to"]);
                label.WithCssClass(BootstrapHelper.Label);
            })
            .Append(HtmlTag.Select, select =>
            {
                select.WithNameAndId($"{dataExportation.Name}{ExportOptions.FileName}");
                select.WithOnChange($"DataExportationHelper.showOptions('{dataExportation.Name}',this.value);");
                select.WithCssClass("form-control form-select");
                select.Append(HtmlTag.Option, option =>
                {
                    option.WithValue($"{(int)ExportFileExtension.XLS}");
                    option.WithAttribute("selected", "selected");
                    option.AppendText("Excel");
                });
                if (PdfWriterExists())
                {
                    select.Append(HtmlTag.Option, option =>
                    {
                        option.WithValue($"{(int)ExportFileExtension.PDF}");
                        option.AppendText("PDF");
                    });
                }

                select.Append(HtmlTag.Option, option =>
                {
                    option.WithValue($"{(int)ExportFileExtension.CSV}");
                    option.AppendText("CSV");
                });
                select.Append(HtmlTag.Option, option =>
                {
                    option.WithValue($"{(int)ExportFileExtension.TXT}");
                    option.AppendText("TXT");
                });
            }
        );
    }

    private HtmlBuilder GetOrientationField()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("col-sm-4")
            .WithAttribute("id", $"{dataExportation.Name}-div-export-orientation")
            .WithStyle( "display:none")
            .WithCssClass(BootstrapHelper.FormGroup)
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", $"{dataExportation.Name}{ExportOptions.TableOrientation}");
                label.WithCssClass(BootstrapHelper.Label);
                label.AppendText(_stringLocalizer["Orientation"]);
            })
        .Append(HtmlTag.Select, select =>
        {
            select.WithNameAndId($"{dataExportation.Name}{ExportOptions.TableOrientation}");
            select.WithCssClass("form-control form-select");
            select.Append(HtmlTag.Option, option =>
            {
                option.WithValue("1");
                option.WithAttribute("selected", "selected");
                option.AppendText("Landscape");
            });
            select.Append(HtmlTag.Option, option =>
            {
                option.WithValue("0");
                option.AppendText("Portrait");
            });
        });
    }

    private HtmlBuilder GetExportAllField()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", $"{dataExportation.Name}-div-export-all")
            .WithCssClass("col-sm-4")
            .WithCssClass(BootstrapHelper.FormGroup)
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", $"{dataExportation.Name}{ExportOptions.ExportAll}");
                label.WithCssClass(BootstrapHelper.Label);
                label.AppendText(_stringLocalizer["Fields"]);
            })
            .Append(HtmlTag.Select, select =>
            {
                select.WithNameAndId($"{dataExportation.Name}{ExportOptions.ExportAll}");
                select.WithCssClass("form-control form-select");
                select.Append(HtmlTag.Option, option =>
                {
                    option.WithValue("1");
                    option.WithAttribute("selected", "selected");
                    option.AppendText(_stringLocalizer["All"]);
                });
                select.Append(HtmlTag.Option, option =>
                {
                    option.WithValue("2");
                    option.AppendText(_stringLocalizer["Only the fields visible on the screen"]);
                });
            });
    }

    private HtmlBuilder GetDelimiterField()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithStyle( "display:none;")
            .WithCssClass("col-sm-4")
            .WithCssClass(BootstrapHelper.FormGroup)
            .WithAttribute("id", $"{dataExportation.Name}-div-export-delimiter")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", $"{dataExportation.Name}{ExportOptions.ExportDelimiter}");
                label.WithCssClass(BootstrapHelper.Label);
                label.AppendText(_stringLocalizer["Delimiter"]);
            })
            .Append(HtmlTag.Select, select =>
            {
                select.WithNameAndId($"{dataExportation.Name}{ExportOptions.ExportDelimiter}");
                select.WithCssClass("form-control form-select");
                select.Append(HtmlTag.Option, option =>
                {
                    option.WithValue(";");
                    option.WithAttribute("selected", "selected");
                    option.AppendText(_stringLocalizer["Semicolon (;)"]);
                });
                select.Append(HtmlTag.Option, option =>
                {
                    option.WithValue(",");
                    option.AppendText(_stringLocalizer["Comma (,)"]);
                });
                select.Append(HtmlTag.Option, option =>
                {
                    option.WithValue("|");
                    option.AppendText(_stringLocalizer["Pipe (|)"]);
                });
            });
    }

    private HtmlBuilder GetFirstLineField()
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass("col-sm-12");
        div.WithCssClass(BootstrapHelper.FormGroup);
        var exportFirstLineCheckbox = dataExportation.ComponentFactory.Controls.CheckBox.Create();
        exportFirstLineCheckbox.Name = $"{dataExportation.Name}{ExportOptions.ExportTableFirstLine}";
        exportFirstLineCheckbox.IsChecked = dataExportation.ExportOptions.ExportFirstLine;
        exportFirstLineCheckbox.Layout = CheckboxLayout.Switch;
        exportFirstLineCheckbox.Text = _stringLocalizer["Export first line as title"];
        div.Append(exportFirstLineCheckbox.GetHtmlBuilder());
        
        return div;
    }

    private JJAlert GetTooManyRecordsAlert(string name)
    {
        var alert = new JJAlert
        {
            ShowIcon = true,
            Name = $"data-exportation-warning{name}",
            ShowCloseButton = true,
            Title = _stringLocalizer["Warning!"],
            Icon = FontAwesomeIcon.ExclamationTriangle,
            Color = BootstrapColor.Warning,
            Messages =
            {
                _stringLocalizer[
                    "You are trying to export more than 50,000 records, this can cause system overhead and slowdowns."],
                _stringLocalizer[
                    "Use filters to reduce export volume, if you need to perform this operation frequently, contact your system administrator."]
            }
        };
        alert.SetAttribute("style", "display:none;");
        return alert;
    }

    private async Task<HtmlBuilder> GetFilesCollapsePanelHtmlBuilderAsync(string exportationFolderPath)
    {
        var files = await GetGeneratedFilesAsync(exportationFolderPath);
        var filesCount = files.Count;
        var panel = new JJCollapsePanel
        {
            Name = "exportCollapse",
            ExpandedByDefault = false,
            TitleIcon = new JJIcon(FontAwesomeIcon.FolderOpenO),
            Visible = filesCount  > 0,
            Title = @$"{_stringLocalizer["Recently generated files"]} ({filesCount})",
            Content = GetLastFilesHtml(files)
        };

        return panel.GetHtmlBuilder()?.WithCssClass($"col-sm-12 {BootstrapHelper.FormGroup}");
    }

    private HtmlBuilder GetLastFilesHtml(List<FileStorageItem> files)
    {
        if (files == null || files.Count == 0)
            return new HtmlBuilder(_stringLocalizer["No recently generated files."]);

        var html = new HtmlBuilder(HtmlTag.Div);
        foreach (var file in files)
        {
            var icon = JJDataExportation.GetFileIcon(Path.GetExtension(file.FileName));
            string url = dataExportation.GetDownloadUrl(file.FileName);

            var div = new HtmlBuilder(HtmlTag.Div);
            div.WithCssClass("mb-1");
            div.AppendComponent(icon);
            div.AppendText(" ");
            div.Append(HtmlTag.A, a =>
            {
                a.WithAttribute("href", url);
                a.WithAttribute("title", "Download");
                a.AppendText(file.FileName);
            });

            html.Append(div);
        }

        return html;
    }

    private async Task<List<FileStorageItem>> GetGeneratedFilesAsync(string exportationFolderPath)
    {
        return (await dataExportation.FileStorage.ListAsync(exportationFolderPath))
            .OrderByDescending(f => f.LastWriteTime)
            .ToList();
    }

    private bool PdfWriterExists() => dataExportation.DataExportationWriterFactory.PdfWriterExists();
}
