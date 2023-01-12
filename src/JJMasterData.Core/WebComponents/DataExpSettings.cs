using System.Collections.Generic;
using System.IO;
using System.Linq;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.DI;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

internal class DataExpSettings
{
    private readonly JJDataExp _dataExp;

    private readonly string _colSm = BootstrapHelper.Version > 3 ? "col-sm-2" : "col-sm-4";
    private readonly string _bs4Row = BootstrapHelper.Version > 3 ? "row" : string.Empty;

    private readonly string _bsLabel = BootstrapHelper.Version > 3 ? BootstrapHelper.Label + "  form-label" : string.Empty;

    public DataExpSettings(JJDataExp dataExp)
    {
        _dataExp = dataExp;
    }

    internal HtmlBuilder GetHtmlElement()
    {
        var html = new HtmlBuilder(HtmlTag.Div);

        html.AppendElement(GetFormHtmlElement());

        html.AppendElement(HtmlTag.Hr);
        html.AppendElement(HtmlTag.Div, div =>
        {
            div.WithCssClass("row");
            div.AppendElement(HtmlTag.Div, div =>
            {
                var btnOk = new JJLinkButton
                {
                    Text = Translate.Key("Export"),
                    IconClass = "fa fa-check",
                    ShowAsButton = true,
                    OnClientClick = $"JJDataExp.doExport('{_dataExp.Name}');"
                };

                var btnCancel = new JJLinkButton
                {
                    Text = "Cancel",
                    IconClass = "fa fa-times",
                    ShowAsButton = true
                };

                btnCancel.Attributes.Add(BootstrapHelper.DataDismiss, "modal");

                div.WithCssClass($"col-sm-12 {BootstrapHelper.TextRight}");
                div.AppendElement(btnOk);
                div.AppendText("&nbsp;");
                div.AppendElement(btnCancel);
            });
        });

        return html;
    }

    private HtmlBuilder GetFormHtmlElement()
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass(BootstrapHelper.FormHorizontal);
        div.WithAttribute("role", "form");

        div.AppendElement(HtmlTag.Div, div =>
        {
            div.AppendElement(GetFileExtensionField());

            div.AppendElement(GetOrientationField());

            div.AppendElement(GetExportAllField());

            div.AppendElement(GetDelimiterField());

            div.AppendElement(GetFirstLineField());

            div.AppendElement(GetFilesPanelHtmlElement());

            div.AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass("row");
                div.AppendElement(HtmlTag.Label, label =>
                {
                    label.WithCssClass($"small {_bsLabel} col-sm-12");
                    label.AppendElement(HtmlTag.Br);
                    label.AppendElement(new JJIcon()
                    {
                        IconClass = "text-info fa fa-info-circle"
                    });
                    label.AppendText(
                        "&nbsp;" + Translate.Key("Filters performed in the previous screen will be considered in the export"));
                });
            });

            div.AppendElement(GetTooManyRecordsAlert(_dataExp.Name));
        });

        return div;
    }

    private HtmlBuilder GetFileExtensionField()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} {_bs4Row}")
            .AppendElement(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", $"{_dataExp.Name}{ExportOptions.FileName}");
                label.WithCssClass($"{_bsLabel} col-sm-4");
                label.AppendText(Translate.Key("Export to"));
            })
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass(_colSm);
                div.AppendElement(HtmlTag.Select, select =>
                {
                    select.WithNameAndId($"{_dataExp.Name}{ExportOptions.FileName}");
                    select.WithAttribute("onchange", $"jjview.showExportOptions('{_dataExp.Name}',this.value);");
                    select.WithCssClass("form-control form-select");
                    select.AppendElement(HtmlTag.Option, option =>
                    {
                        option.WithValue($"{(int)ExportFileExtension.XLS}");
                        option.WithAttribute("selected", "selected");
                        option.AppendText("Excel");
                    });
                    if (PdfWriterExists())
                    {
                        select.AppendElement(HtmlTag.Option, option =>
                        {
                            option.WithValue($"{(int)ExportFileExtension.PDF}");
                            option.AppendText("PDF");
                        });
                    }

                    select.AppendElement(HtmlTag.Option, option =>
                    {
                        option.WithValue($"{(int)ExportFileExtension.CSV}");
                        option.AppendText("CSV");
                    });
                    select.AppendElement(HtmlTag.Option, option =>
                    {
                        option.WithValue($"{(int)ExportFileExtension.TXT}");
                        option.AppendText("TXT");
                    });
                });
            });
    }

    private HtmlBuilder GetOrientationField()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} {_bs4Row}")
            .WithAttribute("id", $"{_dataExp.Name}_div_export_orientation")
            .WithAttribute("style", "display:none")
            .AppendElement(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", $"{_dataExp.Name}{ExportOptions.TableOrientation}");
                label.WithCssClass($"{_bsLabel} col-sm-4");
                label.AppendText(Translate.Key("Orientation"));
            })
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass(_colSm);
                div.AppendElement(HtmlTag.Select, select =>
                {
                    select.WithNameAndId($"{_dataExp.Name}{ExportOptions.TableOrientation}");
                    select.WithCssClass("form-control form-select");
                    select.AppendElement(HtmlTag.Option, option =>
                    {
                        option.WithValue("1");
                        option.WithAttribute("selected", "selected");
                        option.AppendText("Landscape");
                    });
                    select.AppendElement(HtmlTag.Option, option =>
                    {
                        option.WithValue("0");
                        option.AppendText("Portrait");
                    });
                });
            });
    }

    private HtmlBuilder GetExportAllField()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} {_bs4Row}")
            .WithAttribute("id", $"{_dataExp.Name}_div_export_all")
            .AppendElement(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", $"{_dataExp.Name}{ExportOptions.ExportAll}");
                label.WithCssClass($"{_bsLabel} col-sm-4");
                label.AppendText(Translate.Key("Fields"));
            })
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass(_colSm);
                div.AppendElement(HtmlTag.Select, select =>
                {
                    select.WithNameAndId($"{_dataExp.Name}{ExportOptions.ExportAll}");
                    select.WithCssClass("form-control form-select");
                    select.AppendElement(HtmlTag.Option, option =>
                    {
                        option.WithValue("1");
                        option.WithAttribute("selected", "selected");
                        option.AppendText(Translate.Key("All"));
                    });
                    select.AppendElement(HtmlTag.Option, option =>
                    {
                        option.WithValue("2");
                        option.AppendText(Translate.Key("Only the fields visible on the screen"));
                    });
                });
            });
    }

    private HtmlBuilder GetDelimiterField()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} {_bs4Row}")
            .WithAttribute("style", "display:none;")
            .WithAttribute("id", $"{_dataExp.Name}_div_export_delimiter")
            .AppendElement(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", $"{_dataExp.Name}{ExportOptions.ExportDelimiter}");
                label.WithCssClass($"{_bsLabel} col-sm-4");
                label.AppendText(Translate.Key("Delimiter"));
            })
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass(_colSm);
                div.AppendElement(HtmlTag.Select, select =>
                {
                    select.WithNameAndId($"{_dataExp.Name}{ExportOptions.ExportDelimiter}");
                    select.WithCssClass("form-control form-select");
                    select.AppendElement(HtmlTag.Option, option =>
                    {
                        option.WithValue(";");
                        option.WithAttribute("selected", "selected");
                        option.AppendText(Translate.Key("Semicolon (;)"));
                    });
                    select.AppendElement(HtmlTag.Option, option =>
                    {
                        option.WithValue(",");
                        option.AppendText(Translate.Key("Comma (,)"));
                    });
                    select.AppendElement(HtmlTag.Option, option =>
                    {
                        option.WithValue("|");
                        option.AppendText(Translate.Key("Pipe (|)"));
                    });
                });
            });
    }

    private HtmlBuilder GetFirstLineField()
    {
        return new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} {_bs4Row}")
            .WithAttribute("id", $"{_dataExp.Name}_div_export_fistline")
            .AppendElement(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", $"{_dataExp.Name}{ExportOptions.ExportTableFirstLine}");
                label.WithCssClass($"{_bsLabel} col-sm-4");
                label.AppendText(Translate.Key("Export first line as title"));
            })
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass(_colSm);
                div.AppendElement(HtmlTag.Input, input =>
                {
                    input.WithAttribute("type", "checkbox");
                    input.WithValue("1");
                    input.WithCssClass("form-control");
                    input.WithNameAndId($"{_dataExp.Name}{ExportOptions.ExportTableFirstLine}");
                    input.WithAttribute("data-toggle", "toggle");
                    input.WithAttribute("data-on", Translate.Key("Yes"));
                    input.WithAttribute("data-off", Translate.Key("No"));
                    if (_dataExp.ExportOptions.ExportFirstLine)
                        input.WithAttribute("checked", "checked");
                });
            });
    }

    private JJAlert GetTooManyRecordsAlert(string name)
    {
        var alert = new JJAlert
        {
            ShowIcon = true,
            Name = $"warning_exp_{name}",
            ShowCloseButton = true,
            Title = "Warning!",
            Icon = IconType.ExclamationTriangle,
            Color = PanelColor.Warning,
            Messages =
            {
                Translate.Key(
                    "You are trying to export more than 50,000 records, this can cause system overhead and slowdowns."),
                Translate.Key(
                    "Use filters to reduce export volume, if you need to perform this operation frequently, contact your system administrator.")
            }
        };
        alert.SetAttr("style", "display:none;");
        return alert;
    }

    private JJCollapsePanel GetFilesPanelHtmlElement()
    {
        var files = GetGeneratedFiles();
        var panel = new JJCollapsePanel
        {
            Name = "exportCollapse",
            ExpandedByDefault = false,
            TitleIcon = new JJIcon(IconType.FolderOpenO),
            Title = Translate.Key("Recently generated files") + $" ({files.Count})",
            HtmlBuilderContent = GetLastFilesHtmlElement(files)
        };

        return panel;
    }

    private HtmlBuilder GetLastFilesHtmlElement(List<FileInfo> files)
    {
        if (files == null || files.Count == 0)
            return new HtmlBuilder(Translate.Key("No recently generated files."));

        var html = new HtmlBuilder(HtmlTag.Div);
        foreach (var file in files)
        {
            if (FileIO.IsFileLocked(file))
                continue;

            var icon = _dataExp.GetFileIcon(file.Extension);
            string url = JJDataExp.GetDownloadUrl(file.FullName);

            var div = new HtmlBuilder(HtmlTag.Div);
            div.WithCssClass("mb-1");
            div.AppendElement(icon);
            div.AppendText("&nbsp;");
            div.AppendElement(HtmlTag.A, a =>
            {
                a.WithAttribute("href", url);
                a.WithAttribute("title", "Download");
                a.AppendText(file.Name);
            });

            html.AppendElement(div);
        }

        return html;
    }

    private List<FileInfo> GetGeneratedFiles()
    {
        var list = new List<FileInfo>();

        var oDir = new DirectoryInfo(JJServiceCore.CoreOptions.ExportationFolderPath);

        if (oDir.Exists)
            list.AddRange(oDir.GetFiles("*", SearchOption.AllDirectories));

        return list.OrderByDescending(f => f.CreationTime).ToList();
    }

    private bool PdfWriterExists() => WriterFactory.GetPdfWriter() != null;
}