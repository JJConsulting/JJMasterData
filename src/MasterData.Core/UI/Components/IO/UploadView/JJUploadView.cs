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
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Events.Args;

using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

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
    private const string FileName = "Name";
    private const string FileNameJs = "NameJS";

    public event AsyncEventHandler<FormUploadFileEventArgs> OnBeforeCreateFileAsync
    {
        add => FormFileService.OnBeforeCreateFileAsync += value;
        remove => FormFileService.OnBeforeCreateFileAsync -= value;
    }

    public event AsyncEventHandler<FormDeleteFileEventArgs> OnBeforeDeleteFileAsync
    {
        add => FormFileService.OnBeforeDeleteFileAsync += value;
        remove => FormFileService.OnBeforeDeleteFileAsync -= value;
    }

    public event AsyncEventHandler<FormRenameFileEventArgs> OnBeforeRenameFileAsync
    {
        add => FormFileService.OnBeforeRenameFileAsync += value;
        remove => FormFileService.OnBeforeRenameFileAsync -= value;
    }
    public event EventHandler<FormDownloadFileEventArgs> OnBeforeDownloadFile;
    
    internal string ParentName { get; set; }
    public bool ShowAddFiles { get; set; }
    
    public bool IsCollapseExpandedByDefault { get; set; }
    
    public string Title { get; set; }
    public string SubTitle { get; set; }
    public HeadingSize TitleSize { get; set; } = HeadingSize.H3;
    public FontAwesomeIcon? Icon { get; set; }
    
    public bool ViewGallery { get; set; }

    public Dictionary<string, object> UserValues { get; set; } = new();
    
    /// <summary>
    /// Always apply changes to the file system.
    /// If false, keep it in the memory.
    /// (The default value is true.)
    /// </summary>
    public bool AutoSave { get; set; } = true;

    public string FolderPath { get; set; }

    public string DraftId => field ??= FormFileService.GetDraftId($"{Name}-files");

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
            field.QueryStringParams["draftId"] = DraftId;

            return field;
        }
    }
    
    public string JsCallback { get; set; } = "getMasterDataForm().submit()";

    public UrlRedirectAction DownloadAction =>
        field ??= new UrlRedirectAction
        {
            Icon = FontAwesomeIcon.CloudDownload,
            Tooltip = "Download File",
            Name = "download-file"
        };

    public ScriptAction DeleteAction
    {
        get
        {
            if (field != null)
                return field;
            
            field = new ScriptAction
            {
                Icon = FontAwesomeIcon.Trash,
                Tooltip = "Delete File",
                OnClientClick = Scripts.GetDeleteFileScript(),
                Name = "delete-file"
            };
          
            return field;
        }
    }

    public ScriptAction RenameAction
    {
        get
        {
            if (field != null)
                return field;

            field = new ScriptAction
            {
                Icon = FontAwesomeIcon.PencilSquareO,
                Tooltip = "Rename File",
                OnClientClick = Scripts.GetRenameFileScript(),
                Name = "rename-file"
            };
            
            field.SetVisible(false);
            
            return field;
        }
        set;
    }

    private UploadViewScripts Scripts => field ??= new UploadViewScripts(this);

    private IHttpContextAccessor CurrentContext { get; }
    private IComponentFactory ComponentFactory { get; }
    private FormFileService FormFileService { get; }
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
    private ILoggerFactory LoggerFactory { get; }
    private ILogger<JJUploadView> Logger { get; }
    public JJUploadView(
        IHttpContextAccessor currentContext,
        IComponentFactory componentFactory,
        FormFileService formFileService,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    {
        CurrentContext = currentContext;
        ComponentFactory = componentFactory;
        FormFileService = formFileService;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
        Logger = loggerFactory.CreateLogger<JJUploadView>();
        Name = "upload-view";
        ShowAddFiles = true;
        IsCollapseExpandedByDefault = true;
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
        
        return await GetUploadViewResult();
    }

    public async Task<ComponentResult> GetUploadViewResult()
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
        var html = new HtmlBuilder()
           .AppendHiddenInput($"upload-view-action-{Name}")
           .AppendHiddenInput($"upload-view-file-name-{Name}")
           .AppendHiddenInput($"{Name}-files-draft-id", DraftId);

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
            return new JJAlert{Title = StringLocalizer["There is no files to display."]}.GetHtmlBuilder();

        var row = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row");

        foreach (var file in files)
        {
            var col = new HtmlBuilder(HtmlTag.Div);
            col.WithCssClass("col-sm-3");
            
            var previewHtml = await GetHtmlGalleryPreview(file.FileName);
            var actionsHtml = await GetActionsHtmlListAsync(file.FileName);
            
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
            .Append(HtmlTag.B, b =>
            {
                b.AppendText(StringLocalizer[label]);
            })
            .AppendText(value);
    }

    private async Task<HtmlBuilder> GetHtmlGalleryPreview(string fileName)
    {
        var html = new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("list-group-item");

        switch (Path.GetExtension(fileName))
        {
            case ".png":
            case ".jpg":
            case ".jpeg":
                html.Append(await GetHtmlImageBox(fileName));
                break;
            case ".mp4":
                html.WithCssClass("text-center");
                html.Append(GetHtmlVideoBox(fileName));
                break;
            case ".pdf":
                html.WithCssClass("text-center");
                html.WithStyle( "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-pdf-o", "red"));
                break;
            case ".pptx":
                html.WithCssClass("text-center");
                html.WithStyle( "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-powerpoint-o", "red"));
                break;
            case ".docx":
                html.WithCssClass("text-center");
                html.WithStyle( "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-word-o", "blue"));
                break;
            case ".csv":
            case ".txt":
                html.WithCssClass("text-center");
                html.WithStyle( "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-text-o", "black"));
                break;
            case ".xls":
                html.WithCssClass("text-center");
                html.WithStyle( "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-excel-o", "green"));
                break;
            case ".rar":
            case ".zip":
                html.WithCssClass("text-center");
                html.WithStyle( "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-zip-o", "#d2bb1c"));
                break;
            default:
                html.WithCssClass("text-center");
                html.WithStyle( "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-o", "gray"));
                break;
        }

        return html;
    }

    private static HtmlBuilder GetHtmlItemBox(string fileName, string cssIcon, string colorIcon)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithStyle( "height:180px;")
            .Append(HtmlTag.Span, span =>
            {
                span.WithCssClass(cssIcon)
                    .WithToolTip(fileName)
                    .WithStyle( $"color:{colorIcon};padding-top:45px;font-size:100px;");
            });
        return div;
    }

    private async Task<HtmlBuilder> GetHtmlImageBox(string fileName)
    {
        var downloader = ComponentFactory.Downloader.Create();
        downloader.File = await FormFileService.GetFileKeyAsync(DraftId, FolderPath, fileName);
        var src = downloader.GetDownloadUrl();
        
        var html = new HtmlBuilder(HtmlTag.Img);

        html.WithAttribute("loading", "lazy")
            .WithAttribute("src", src)
            .WithStyle( "height:180px;")
            .WithCssClass("img-responsive")
            .WithToolTip(fileName);
    
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
                       .WithStyle( "max-height:350px;")
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
            return new JJAlert { Title = StringLocalizer["There is no files to display."] }.GetHtmlBuilder();

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
            var actionsHtml = await GetActionsHtmlListAsync(file.FileName);
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

    private async Task<List<HtmlBuilder>> GetActionsHtmlListAsync(string fileName)
    {
        List<HtmlBuilder> actions = [];

        if (DownloadAction.IsVisible)
        {
            var downloader = ComponentFactory.Downloader.Create();
            downloader.File = await FormFileService.GetFileKeyAsync(DraftId, FolderPath, fileName);
            actions.Add(CreateActionCell(DownloadAction, urlAction: downloader.GetDownloadUrl()));
        }

        if (RenameAction.IsVisible)
            actions.Add(CreateActionCell(RenameAction, onClientClick: GetActionScript(RenameAction, fileName)));

        if (DeleteAction.IsVisible)
            actions.Add(CreateActionCell(DeleteAction, onClientClick: GetActionScript(DeleteAction, fileName)));

        return actions;
    }

    private static HtmlBuilder CreateActionCell(BasicAction action, string urlAction = null, string onClientClick = null)
    {
        var td = new HtmlBuilder(HtmlTag.Td);
        td.WithCssClass("table-action");
        td.AppendComponent(CreateActionButton(action, urlAction, onClientClick));

        return td;
    }

    private static JJLinkButton CreateActionButton(BasicAction action, string urlAction = null, string onClientClick = null)
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

    private static string GetActionScript(ScriptAction action, string fileName)
    {
        var fileNameJs = HttpUtility.JavaScriptStringEncode(fileName);
        return action.OnClientClick?.Replace($"{{{FileNameJs}}}", fileNameJs);
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
      FormFileService.RenameFileAsync(DraftId, FolderPath, currentName, newName);

    public Task CreateFileAsync(FormFileContent file) =>
        FormFileService.CreateFileAsync(DraftId, FolderPath, AutoSave, file, !UploadArea.Multiple);

    public async Task<ComponentResult> GetDeleteFileResultAsync(string fileName)
    {
        await FormFileService.DeleteFileAsync(DraftId, FolderPath, fileName);
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
        FormFileService.DeleteAllAsync(DraftId, FolderPath, AutoSave);

    public Task<List<FileStorageItem>> GetFilesAsync() => 
        FormFileService.GetFilesAsync(DraftId, FolderPath, !UploadArea.Multiple);

    public Task ClearTemporaryFilesAsync() => 
        FormFileService.DeleteAllAsync(DraftId, FolderPath, AutoSave);

    public Task PromoteTemporaryFilesAsync(string folderPath) =>
        FormFileService.PromoteTemporaryFilesAsync(DraftId, folderPath, !UploadArea.Multiple);

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
        var downloader = ComponentFactory.Downloader.Create();
        downloader.File = await FormFileService.GetFileKeyAsync(DraftId, FolderPath, fileName);
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

}
