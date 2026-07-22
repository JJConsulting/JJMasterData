#nullable disable warnings
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

// ReSharper disable MemberCanBePrivate.Global
namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Form responsible for managing files in a directory
/// </summary>
/// <example>
/// The output will look like this:
/// <img src="../media/JJFormUploadFileExample.png"/>
/// </example>
/// <seealso cref="JJUploadArea"/>
public class JJUploadView : AsyncComponent
{
    private const string FileNameJs = "NameJS";

    internal const string DownloadFileActionName = "download-file";
    internal const string DeleteFileActionName = "delete-file";
    internal const string RenameFileActionName = "rename-file";

    internal const string TempPathFolder = "{app.path}/MasterDataDraftFiles/";
    
    public event AsyncEventHandler<FormUploadFileEventArgs> OnBeforeCreateFileAsync
    {
        add => Manager.OnBeforeCreateFileAsync += value;
        remove => Manager.OnBeforeCreateFileAsync -= value;
    }

    public event AsyncEventHandler<FormDeleteFileEventArgs> OnBeforeDeleteFileAsync
    {
        add => Manager.OnBeforeDeleteFileAsync += value;
        remove => Manager.OnBeforeDeleteFileAsync -= value;
    }

    public event AsyncEventHandler<FormRenameFileEventArgs> OnBeforeRenameFileAsync
    {
        add => Manager.OnBeforeRenameFileAsync += value;
        remove => Manager.OnBeforeRenameFileAsync -= value;
    }

    public event EventHandler<FormDownloadFileEventArgs> OnBeforeDownloadFile;

    internal string ParentName { get; set; }
    internal string DeletedFilesInputName { get; set; }

    public bool ShowAddFiles { get; set; }

    public bool IsCollapseExpandedByDefault { get; set; }

    public string Title { get; set; }

    public string SubTitle { get; set; }

    public HeadingSize TitleSize { get; set; } = HeadingSize.H3;

    public FontAwesomeIcon? Icon { get; set; }

    public bool ViewGallery { get; set; }

    public string EmptyDataText { get; set; } = "There is no files to display.";

    /// <summary>
    /// Always apply changes to the file system.
    /// If false, keep it in the temp folder.
    /// (The default value is true.)
    /// </summary>
    public bool AutoSave { get; set; } = true;

    /// <summary>
    /// Full Directory Path.
    /// (Optional) If the path is not given, all files will be stored in the temp folder.
    /// </summary>
    /// <remarks>
    /// Example: C:\temp\files\ (Windows) or /tmp/Files (Linux)
    /// </remarks>
    public string FolderPath { get; set; }


    public string TempPath => TempPathFolder + DraftId.ToString("N") +  "/";


    internal Guid DraftId
    {
        get
        {
            var draftIdStr = CurrentContext.HttpContext!.Request.GetValue($"{Name}-draft-id");
            if (!string.IsNullOrWhiteSpace(draftIdStr))
                return Guid.Parse(draftIdStr);

            return Guid.NewGuid();
        }
    }
    
    public JJUploadArea UploadArea
    {
        get
        {
            if (field != null)
                return field;

            field = ComponentFactory.UploadArea.Create();
            field.OnFileUploadedAsync += OnFileUploadedAsync;
            field.JsCallback = JsCallback;
            field.Name = $"{Name}-files";
            field.QueryStringParams[$"{Name}-draft-id"] = DraftId.ToString("N");

            return field;
        }
    }

    public string JsCallback
    {
        get;
        set
        {
            field = value;
            UploadArea?.JsCallback = value;
        }
    } = "getMasterDataForm().submit()";

    public UrlRedirectAction DownloadAction { get; } = new()
    {
        Icon = FontAwesomeIcon.CloudDownload,
        Tooltip = "Download File",
        Name = DownloadFileActionName
    };
    

    public ScriptAction DeleteAction { get; } = new()
    {
        Icon = FontAwesomeIcon.Trash,
        Tooltip = "Delete File",
        Name = DeleteFileActionName
    };

    public ScriptAction RenameAction { get; } = new()
    {
        Icon = FontAwesomeIcon.PencilSquareO,
        Tooltip = "Rename File",
        Name = RenameFileActionName
    };

    private UploadViewScripts Scripts => field ??= new UploadViewScripts(this);

    private IHttpContextAccessor CurrentContext { get; }
    private IComponentFactory ComponentFactory { get; }
    private UploadViewManager Manager { get; }
    private IEncryptionService EncryptionService { get; }

    protected RouteContext RouteContext
    {
        get
        {
            if (field != null)
                return field;

            var factory = new RouteContextFactory(CurrentContext, EncryptionService);
            field = factory.Create();

            return field;
        }
    }

    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; }

    private ILogger<JJUploadView> Logger { get; }

    public JJUploadView(
        IHttpContextAccessor currentContext,
        IComponentFactory componentFactory,
        UploadViewManager manager,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    {
        CurrentContext = currentContext;
        ComponentFactory = componentFactory;
        Manager = manager;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        Logger = loggerFactory.CreateLogger<JJUploadView>();
        Name = "upload-view";
        ShowAddFiles = true;
        IsCollapseExpandedByDefault = true;

        RenameAction.SetVisible(false);
    }


    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (RouteContext.ComponentContext is ComponentContext.DownloadFile)
        {
            var downloader = ComponentFactory.Downloader.Create();
            return await downloader.GetDownloadResultAsync();
        }

        var uploadAreaResult = await UploadArea.GetResultAsync();

        if (uploadAreaResult is JsonComponentResult)
        {
            return uploadAreaResult;
        }

        return await GetUploadViewResult(appendHiddenInputs: true);
    }

    public async Task<ComponentResult> GetUploadViewResult(bool appendHiddenInputs)
    {
        var html = HtmlBuilder.Div();

        var uploadAction = CurrentContext.HttpContext!.Request.GetFormValue($"upload-view-action-{Name}");
        if (!string.IsNullOrEmpty(uploadAction))
        {
            var result = await GetUploadActionResultAsync(uploadAction);

            if (result is RenderedComponentResult renderedComponent)
            {
                html.Append(renderedComponent.HtmlBuilder);
            }
            else
            {
                return result;
            }
        }

        if (!string.IsNullOrEmpty(Title))
        {
            var title = new JJTitle
            {
                Title = Title,
                SubTitle = SubTitle,
                Icon = Icon,
                Size = TitleSize
            };
            html.AppendComponent(title);
        }

        html.Append(GetUploadAreaHtml());

        if (appendHiddenInputs)
            html.Append(GetHiddenInputsHtmlAsync());

        if (ViewGallery)
        {
            html.Append(await GetGalleryHtml());
        }
        else
        {
            html.Append(await GetFilesTableHtmlAsync());
        }

        html.AppendComponent(await GetPreviewModalHtml());

        html.WithId(Name);

        return new RenderedComponentResult(html);
    }

    private async Task<ComponentResult> GetUploadActionResultAsync(string uploadViewAction)
    {
        var fileName = CurrentContext.HttpContext!.Request.GetFormValue($"upload-view-file-name-{Name}");
        try
        {
            switch (uploadViewAction)
            {
                case "deleteFile":
                    return await GetDeleteFileResultAsync(fileName);
                case "downloadFile":
                    return await GetDownloadFileResultAsync(fileName);
                case "renameFile":
                    return await GetRenameFileResultAsync(fileName);
            }
        }
        catch (Exception ex)
        {
            var alert = new JJAlert
            {
                Title = ex.Message,
                Color = BootstrapColor.Warning,
                ShowCloseButton = true,
                Icon = FontAwesomeIcon.SolidTriangleExclamation
            };

            return new RenderedComponentResult(alert.GetHtmlBuilder());
        }

        throw new InvalidOperationException("Invalid JJUploadView action.");
    }

    private HtmlBuilder GetUploadAreaHtml()
    {
        var html = new HtmlBuilder();

        if (!ShowAddFiles)
            return html;

        html.AppendComponent(new JJMasterDataCollapsePanel(CurrentContext)
        {
            Title = StringLocalizer["New File"],
            ExpandedByDefault = IsCollapseExpandedByDefault,
            Content = GetHtmlFormPanel()
        });

        return html;
    }

    internal HtmlBuilder GetHiddenInputsHtmlAsync()
    {
        var html = new HtmlBuilder()
            .AppendHiddenInput($"upload-view-action-{Name}")
            .AppendHiddenInput($"upload-view-file-name-{Name}")
            .AppendHiddenInput($"{Name}-draft-id", DraftId.ToString("N"));

        if (!AutoSave && !string.IsNullOrWhiteSpace(DeletedFilesInputName))
            html.AppendHiddenInput(DeletedFilesInputName, GetDeletedFilesByComma());

        return html;
    }

    private HtmlBuilder GetHtmlFormPanel()
    {
        var panelContent = new HtmlBuilder();
        if (!UploadArea.AllowedTypes.Equals("*"))
        {
            var label = new JJLabel
            {
                Text = $"{StringLocalizer["File Type:"]}\u00A0<b>{UploadArea.AllowedTypes}</b>"
            };
            panelContent.AppendComponent(label);
        }

        panelContent.Append(UploadArea.GetUploadAreaHtmlBuilder());
        return panelContent;
    }

    private async Task<HtmlBuilder> GetGalleryHtml()
    {
        var files = await GetFilesAsync();
        if (files.Count == 0)
            return new JJAlert { Title = StringLocalizer[EmptyDataText] }.GetHtmlBuilder();

        var row = HtmlBuilder.Div().WithCssClass("row");

        foreach (var file in files)
        {
            var col = HtmlBuilder.Div();
            col.WithCssClass("col-sm-3");

            var previewHtml = GetHtmlGalleryPreview(file);
            var actionsHtml = GetFileActions(file);

            col.Append(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("list-group list-group-flush");
                ul.Append(previewHtml);

                ul.AppendRange(GetGalleryListItems(file));

                ul.Append(HtmlTag.Li, li =>
                {
                    li.WithCssClass("list-group-item");
                    li.Append(HtmlTag.Table, table =>
                    {
                        table.WithCssClass("table-gallery");
                        table.Append(HtmlTag.Tr, tr =>
                        {
                            tr.AppendRange(actionsHtml);
                        });
                    });
                });
            });

            row.Append(col);
        }

        return row;
    }

    private IEnumerable<HtmlBuilder> GetGalleryListItems(FileStorageItem file)
    {
        yield return GetHtmlGalleryListItem("Name", file.FileName);
        yield return GetHtmlGalleryListItem("Size", $"{file.Length} Bytes");
        yield return GetHtmlGalleryListItem("Last Modified", file.LastWriteTime.ToString(CultureInfo.CurrentCulture));
    }

    private HtmlBuilder GetHtmlGalleryListItem(string label, string value)
    {
        return new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("list-group-item")
            .Append(HtmlTag.B, b => { b.AppendText(StringLocalizer[label]); })
            .AppendText(value);
    }

    private HtmlBuilder GetHtmlGalleryPreview(FileStorageItem file)
    {
        var html = new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("list-group-item");

        var fileName = file.FileName;
        switch (Path.GetExtension(fileName))
        {
            case ".png":
            case ".jpg":
            case ".jpeg":
                html.Append(GetHtmlImageBox(file));
                break;
            case ".mp4":
                html.WithCssClass("text-center");
                html.Append(GetHtmlVideoBox(fileName));
                break;
            case ".pdf":
                html.WithCssClass("text-center");
                html.WithStyle("background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-pdf-o", "red"));
                break;
            case ".pptx":
                html.WithCssClass("text-center");
                html.WithStyle("background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-powerpoint-o", "red"));
                break;
            case ".docx":
                html.WithCssClass("text-center");
                html.WithStyle("background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-word-o", "blue"));
                break;
            case ".csv":
            case ".txt":
                html.WithCssClass("text-center");
                html.WithStyle("background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-text-o", "black"));
                break;
            case ".xls":
                html.WithCssClass("text-center");
                html.WithStyle("background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-excel-o", "green"));
                break;
            case ".rar":
            case ".zip":
                html.WithCssClass("text-center");
                html.WithStyle("background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-zip-o", "#d2bb1c"));
                break;
            default:
                html.WithCssClass("text-center");
                html.WithStyle("background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-o", "gray"));
                break;
        }

        return html;
    }

    private static HtmlBuilder GetHtmlItemBox(string fileName, string cssIcon, string colorIcon)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithStyle("height:180px;")
            .Append(HtmlTag.Span, span =>
            {
                span.WithCssClass(cssIcon)
                    .WithToolTip(fileName)
                    .WithStyle($"color:{colorIcon};padding-top:45px;font-size:100px;");
            });
        return div;
    }

    private HtmlBuilder GetHtmlImageBox(FileStorageItem file)
    {
        var downloader = ComponentFactory.Downloader.Create();

        downloader.FullPath = file.FullPath;
        var src = downloader.GetDownloadUrl();

        var html = new HtmlBuilder(HtmlTag.Img);

        html.WithAttribute("loading", "lazy")
            .WithAttribute("src", src)
            .WithStyle("height:180px;")
            .WithCssClass("img-responsive")
            .WithToolTip(file.FileName);

        return html;
    }

    private static HtmlBuilder GetHtmlVideoBox(string fileName)
    {
        var html = GetHtmlItemBox(fileName, "fa fa-play-circle", "red");

        return html;
    }

    private async Task<JJModalDialog> GetPreviewModalHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div);

        var label = new JJLabel
        {
            Text = "File name",
            LabelFor = $"preview_filename-{UploadArea.Name}"
        };

        var group = ComponentFactory.Controls.TextGroup.Create();
        group.Name = $"preview_filename-{UploadArea.Name}";
        group.Addons = new InputAddons(".png");
        group.Text = "image";

        var groupHtml = await group.GetHtmlBuilderAsync();

        html.Append(HtmlTag.Div, row =>
        {
            row.WithCssClass("row");
            row.Append(HtmlTag.Div, col =>
            {
                col.WithCssClass("col-sm-12");
                col.AppendComponent(label);
                col.Append(groupHtml);
            });
        });

        html.Append(HtmlTag.Div, row =>
        {
            row.WithCssClass("row");
            row.Append(HtmlTag.Div, col =>
            {
                col.WithCssClass("col-sm-12");
                col.Append(HtmlTag.Hr);
            });
            row.Append(HtmlTag.Div, col =>
            {
                col.WithCssClass("col-sm-12");
                col.Append(HtmlTag.Img, img =>
                {
                    img.WithAttribute("id", "pastedimage_0")
                        .WithStyle("max-height:350px;")
                        .WithAttribute("alt", StringLocalizer["Preview Image"])
                        .WithCssClass("img-responsive");
                });
            });
        });

        var btnOk = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            Name = $"btnDoUpload_{UploadArea.Name}",
            CssClass = "btn btn-primary",
            Text = StringLocalizer["Save"]
        };

        var btnCancel = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            Text = StringLocalizer["Cancel"]
        };
        btnCancel.SetAttribute(BootstrapHelper.DataDismiss, "modal");

        var modal = new JJModalDialog
        {
            Name = $"preview_modal_{UploadArea.Name}",
            Title = "Would you like to save the image below?",
            Content = html
        };
        modal.Buttons.Add(btnOk);
        modal.Buttons.Add(btnCancel);

        return modal;
    }

    private async Task<HtmlBuilder> GetFilesTableHtmlAsync()
    {
        var files = await GetFilesAsync();

        if (files.Count == 0)
            return new JJAlert { Title = StringLocalizer[EmptyDataText] }.GetHtmlBuilder();

        var table = new HtmlBuilder(HtmlTag.Table)
            .WithCssClass("table table-striped table-hover table-sm");

        var visibleActionCount = GetVisibleActionsCount();

        table.Append(HtmlTag.Thead, thead =>
        {
            thead.Append(HtmlTag.Tr, tr =>
            {
                tr.Append(HtmlTag.Th, th => th.AppendText(StringLocalizer["Name"]));
                tr.Append(HtmlTag.Th, th => th.AppendText(StringLocalizer["Size"]));
                tr.Append(HtmlTag.Th, th => th.AppendText(StringLocalizer["Last Modified"]));

                for (var i = 0; i < visibleActionCount; i++)
                {
                    tr.Append(HtmlTag.Th, th => th.WithCssClass("table-action"));
                }
            });
        });

        var tbody = new HtmlBuilder(HtmlTag.Tbody);
        foreach (var file in files)
        {
            var actionsHtml = GetFileActions(file);
            tbody.Append(HtmlTag.Tr, tr =>
            {
                tr.Append(HtmlTag.Td, td => td.AppendText(file.FileName));
                tr.Append(HtmlTag.Td, td => td.AppendText(Format.FormatFileSize(file.Length)));
                tr.Append(HtmlTag.Td, td => td.AppendText(file.LastWriteTime.ToString(CultureInfo.CurrentCulture)));
                tr.AppendRange(actionsHtml);
            });
        }

        table.Append(tbody);
        return table;
    }

    private int GetVisibleActionsCount()
    {
        var count = 0;

        if (DownloadAction.IsVisible)
            count++;

        if (RenameAction.IsVisible)
            count++;

        if (DeleteAction.IsVisible)
            count++;

        return count;
    }

    private IEnumerable<HtmlBuilder> GetFileActions(FileStorageItem file)
    {
        var isDraft = file.FolderPath.Equals(TempPath);

        if (DownloadAction.IsVisible)
        {
            var downloader = ComponentFactory.Downloader.Create(file.FullPath);
            var actionButton = CreateActionButton(DownloadAction, urlAction: downloader.GetDownloadUrl());
            
            yield return CreateActionCell(actionButton);
        }

        if (RenameAction.IsVisible)
        {
            var actionButton = CreateActionButton(
                RenameAction,
                onClientClick: GetActionScript(Scripts.GetRenameFileScript(), file.FileName));

            if (!AutoSave && !isDraft)
                actionButton.Enabled = false;

            yield return CreateActionCell(actionButton);
        }

        if (DeleteAction.IsVisible)
        {
            var deleteScript = !AutoSave && !isDraft
                ? Scripts.GetMarkDeletedScript()
                : Scripts.GetDeleteFileScript();
            
            var actionButton = CreateActionButton(
                DeleteAction,
                onClientClick: GetActionScript(deleteScript, file.FileName));
            
            yield return CreateActionCell(actionButton);
        }
    }

    private static HtmlBuilder CreateActionCell(JJLinkButton actionButton)
    {
        var td = new HtmlBuilder(HtmlTag.Td);
        td.WithCssClass("table-action");
        td.AppendComponent(actionButton);

        return td;
    }

    private static JJLinkButton CreateActionButton(BasicAction action, string urlAction = null,
        string onClientClick = null)
    {
        return new JJLinkButton
        {
            Text = action.ShowTitle ? action.Text : null,
            Tooltip = action.Tooltip,
            UrlAction = urlAction,
            OnClientClick = onClientClick,
            IconClass = $"{action.Icon.CssClass} fa-fw",
            ShowAsButton = action.ShowAsButton,
            CssClass = action.CssClass,
            Visible = action.IsVisible
        };
    }

    private static string GetActionScript(string script, string fileName)
    {
        var fileNameJs = HttpUtility.JavaScriptStringEncode(fileName);
        return script.Replace($"{{{FileNameJs}}}", fileNameJs);
    }

    private async ValueTask OnFileUploadedAsync(object sender, FormUploadFileEventArgs args)
    {
        try
        {
            await CreateFileAsync(args.File);
            args.SuccessMessage = "File successfully created.";
        }
        catch (Exception ex)
        {
            args.ErrorMessage = ex.Message;
        }
    }

    private async Task<RenderedComponentResult> GetRenameFileResultAsync(string fileName)
    {
        var names = fileName.Split(';');
        var currentName = names[0];
        var newName = names[1];
        await RenameFileAsync(currentName, newName);

        var text = StringLocalizer["File successfully renamed."];
        var alert = new JJAlert
        {
            Title = text,
            ShowCloseButton = true,
            Color = BootstrapColor.Info,
            Icon = FontAwesomeIcon.SolidCircleInfo
        };

        return new RenderedComponentResult(alert.GetHtmlBuilder());
    }

    public Task RenameFileAsync(string currentName, string newName) =>
        Manager.RenameFileAsync(TempPath, FolderPath, currentName, newName);

    public Task CreateFileAsync(IFormFile file) =>
        Manager.CreateFileAsync(TempPath, FolderPath, AutoSave, file, UploadArea.Multiple);

    public Task DeleteFileAsync(string fileName) =>
        Manager.DeleteFileAsync(TempPath, FolderPath, fileName);

    private async Task<ComponentResult> GetDeleteFileResultAsync(string fileName)
    {
        await DeleteFileAsync(fileName);
        var text = StringLocalizer["File successfully deleted."];
        var alert = new JJAlert
        {
            Title = text,
            ShowCloseButton = true,
            Color = BootstrapColor.Info,
            Icon = FontAwesomeIcon.SolidCircleInfo
        };

        return new RenderedComponentResult(alert.GetHtmlBuilder());
    }

    internal Task DeleteAllAsync() =>
        Manager.DeleteAllAsync(TempPath, FolderPath, AutoSave);

    public async Task<List<FileStorageItem>> GetFilesAsync()
    {
        var files = await Manager.GetFilesAsync(TempPath, FolderPath);
        
        var deletedFiles = GetDeletedFiles();
        
        if (!AutoSave && deletedFiles.Count > 0)
            files.RemoveAll(file => !file.FolderPath.Equals(TempPath) && deletedFiles.Contains(file.FileName));
      
        return files;
    }

    public Task ClearTemporaryFilesAsync() =>
        Manager.DeleteAllAsync(TempPath, null, false);

    public Task PromoteDraftFilesAsync() =>
        Manager.PromoteDraftFilesAsync(TempPath, FolderPath, GetDeletedFiles());

    public async Task<FileStreamComponentResult> GetDownloadFileResultAsync(string fileName)
    {
        if (OnBeforeDownloadFile != null)
        {
            var args = new FormDownloadFileEventArgs(fileName, null);
            OnBeforeDownloadFile(this, args);


            if (!string.IsNullOrEmpty(args.ErrorMessage))
            {
                var exception = new JJMasterDataException(args.ErrorMessage);
                Logger.LogError(exception, "Error on OnBeforeDownloadFile event");
                throw exception;
            }
        }

        var file = await Manager.GetFileAsync(TempPath, FolderPath, fileName);
        if (file == null)
            throw new JJMasterDataException(StringLocalizer["File not found."]);

        var downloader = ComponentFactory.Downloader.Create(file.FullPath);
        return await downloader.GetDirectDownloadResultAsync();
    }

    /// <summary>
    /// Disable all actions, except the download.
    /// </summary>
    public void Disable()
    {
        ShowAddFiles = false;
        RenameAction.SetVisible(false);
        DeleteAction.SetVisible(false);
        DownloadAction.SetVisible(true);
    }

    private string GetDeletedFilesByComma()
    {
        if (string.IsNullOrWhiteSpace(DeletedFilesInputName))
            return string.Empty;

        return CurrentContext.HttpContext!.Request.GetValue(DeletedFilesInputName) ?? string.Empty;
    }

    private HashSet<string> GetDeletedFiles()
    {
        var values = GetDeletedFilesByComma();
        if (string.IsNullOrWhiteSpace(values))
            return [];
        
        return values
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

}
