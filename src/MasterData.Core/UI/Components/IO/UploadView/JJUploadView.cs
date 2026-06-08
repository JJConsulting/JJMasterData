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
using JJMasterData.Commons.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.DataManager.IO.Storage;
using JJMasterData.Core.DataManager.Models;
using Microsoft.AspNetCore.Http;
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

    private UrlRedirectAction _downloadAction;
    private ScriptAction _deleteAction;
    private ScriptAction _renameAction;
    private JJUploadArea _uploadArea;
    private FormFileManager _formFileManager;
    private UploadViewScripts _scripts;
    private RouteContext _routeContext;
    
    public event AsyncEventHandler<FormUploadFileEventArgs> OnBeforeCreateFileAsync;
    public event AsyncEventHandler<FormDeleteFileEventArgs> OnBeforeDeleteFileAsync;
    public event AsyncEventHandler<FormRenameFileEventArgs> OnBeforeRenameFileAsync;
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

    public string FolderKey { get; set; }

    public string DraftId => FormFileManager.DraftId;

    public JJUploadArea UploadArea
    {
        get
        {
            if (_uploadArea != null)
                return _uploadArea;
            
            _uploadArea = ComponentFactory.UploadArea.Create();
            _uploadArea.OnFileUploadedAsync += OnFileUploadedAsync;
            _uploadArea.JsCallback = JsCallback;
            _uploadArea.Name = $"{Name}-files";
            _uploadArea.QueryStringParams["draftId"] = FormFileManager.DraftId;

            return _uploadArea;
        }
    }
    
    public string JsCallback { get; set; } = "getMasterDataForm().submit()";

    public UrlRedirectAction DownloadAction =>
        _downloadAction ??= new UrlRedirectAction
        {
            Icon = FontAwesomeIcon.CloudDownload,
            Tooltip = "Download File",
            Name = "download-file"
        };

    public ScriptAction DeleteAction
    {
        get
        {
            if (_deleteAction != null)
                return _deleteAction;
            
            _deleteAction = new ScriptAction
            {
                Icon = FontAwesomeIcon.Trash,
                Tooltip = "Delete File",
                OnClientClick = Scripts.GetDeleteFileScript(),
                Name = "delete-file"
            };
          
            return _deleteAction;
        }
    }
    
    public ScriptAction RenameAction
    {
        get
        {
            if (_renameAction != null)
                return _renameAction;
            
            _renameAction = new ScriptAction
            {
                Icon = FontAwesomeIcon.PencilSquareO,
                Tooltip = "Rename File",
                OnClientClick = Scripts.GetRenameFileScript(),
                Name = "rename-file"
            };
            _renameAction.SetVisible(false);
            return _renameAction;
        }
        set => _renameAction = value;
    }

    private FormFileManager FormFileManager
    {
        get
        {
            if (_formFileManager == null)
            {
                _formFileManager = FormFileManagerFactory.Create($"{Name}-files");
                _formFileManager.OnBeforeCreateFileAsync += OnBeforeCreateFileAsync;
                _formFileManager.OnBeforeDeleteFileAsync += OnBeforeDeleteFileAsync;
                _formFileManager.OnBeforeRenameFileAsync += OnBeforeRenameFileAsync;
            }
            _formFileManager.AutoSave = AutoSave;
            _formFileManager.FolderKey = FolderKey;
            return _formFileManager;
        }
    }

    private UploadViewScripts Scripts => _scripts ??= new UploadViewScripts(this);

    private IHttpContextAccessor CurrentContext { get; }
    private IComponentFactory ComponentFactory { get; }
    private FormFileManagerFactory FormFileManagerFactory { get; }
    private IEncryptionService EncryptionService { get; }

    protected RouteContext RouteContext
    {
        get
        {
            if (_routeContext != null)
                return _routeContext;

            var factory = new RouteContextFactory(CurrentContext, EncryptionService);
            _routeContext = factory.Create();
            
            return _routeContext;
        }
    }

    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; }
    private ILoggerFactory LoggerFactory { get; }
    private ILogger<JJUploadView> Logger { get; }
    public JJUploadView(
        IHttpContextAccessor currentContext,
        IComponentFactory componentFactory,
        FormFileManagerFactory formFileManagerFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    {
        CurrentContext = currentContext;
        ComponentFactory = componentFactory;
        FormFileManagerFactory = formFileManagerFactory;
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
           .AppendHiddenInput($"{Name}-files-draft-id", FormFileManager.DraftId);

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
        var files = (await GetFilesAsync()).FindAll(x => !x.Deleted);
        if (files.Count == 0) 
            return new JJAlert{Title = StringLocalizer["There is no files to display."]}.GetHtmlBuilder();

        var row = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row");

        foreach (var fileInfo in files)
        {
            var file = fileInfo.Content;
            var col = new HtmlBuilder(HtmlTag.Div);
            col.WithCssClass("col-sm-3");
            
            var previewHtml = await GetHtmlGalleryPreview(file.FileName);
            var actionsHtml = await GetActionsHtmlAsync(file.FileName);
            
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
                            tr.Append(HtmlTag.Td, td => td.Append(actionsHtml));
                        });
                    });
                });
            });

            row.Append(col);
        }

        return row;
    }
    
    private IEnumerable<HtmlBuilder> GetGalleryListItems(FormFileContent file)
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
        downloader.FileReference = await FormFileManager.GetFileReferenceAsync(fileName);
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
        var files = (await FormFileManager.GetFilesAsync())
            .Where(f => !f.Deleted)
            .Select(f => f.Content)
            .ToList();

        if (files.Count == 0)
            return new JJAlert { Title = StringLocalizer["There is no files to display."] }.GetHtmlBuilder();

        var table = new HtmlBuilder(HtmlTag.Table)
            .WithCssClass("table table-striped table-hover table-sm");

        table.Append(HtmlTag.Thead, thead =>
        {
            thead.Append(HtmlTag.Tr, tr =>
            {
                tr.Append(HtmlTag.Th, th => th.AppendText(StringLocalizer["Name"]));
                tr.Append(HtmlTag.Th, th => th.AppendText(StringLocalizer["Size"]));
                tr.Append(HtmlTag.Th, th => th.AppendText(StringLocalizer["Last Modified"]));
                tr.Append(HtmlTag.Th, th =>
                {
                    th.WithCssClass("table-action");
                    th.AppendText(StringLocalizer["Actions"]);
                });
            });
        });

        var tbody = new HtmlBuilder(HtmlTag.Tbody);
        foreach (var file in files)
        {
            var actionsHtml = await GetActionsHtmlAsync(file.FileName);
            tbody.Append(HtmlTag.Tr, tr =>
            {
                tr.Append(HtmlTag.Td, td => td.AppendText(file.FileName));
                tr.Append(HtmlTag.Td, td => td.AppendText(Format.FormatFileSize(file.Length)));
                tr.Append(HtmlTag.Td, td => td.AppendText(file.LastWriteTime.ToString(CultureInfo.CurrentCulture)));
                tr.Append(HtmlTag.Td, td =>
                {
                    td.WithCssClass("table-action");
                    td.Append(actionsHtml);
                });
            });
        }

        table.Append(tbody);
        return table;
    }

    private async Task<HtmlBuilder> GetActionsHtmlAsync(string fileName)
    {
        var buttonGroup = new JJLinkButtonGroup();

        if (DownloadAction.IsVisible)
        {
            var downloader = ComponentFactory.Downloader.Create();
            downloader.FileReference = await FormFileManager.GetFileReferenceAsync(fileName);
            buttonGroup.Actions.Add(CreateActionButton(DownloadAction, urlAction: downloader.GetDownloadUrl()));
        }

        if (RenameAction.IsVisible)
            buttonGroup.Actions.Add(CreateActionButton(RenameAction, onClientClick: GetActionScript(RenameAction, fileName)));

        if (DeleteAction.IsVisible)
            buttonGroup.Actions.Add(CreateActionButton(DeleteAction, onClientClick: GetActionScript(DeleteAction, fileName)));

        return buttonGroup.GetHtmlBuilder();
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
            CssClass = action.CssClass
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
      FormFileManager.RenameFileAsync(currentName, newName);

    public Task CreateFileAsync(FormFileContent file) =>
        FormFileManager.CreateFileAsync(file, !UploadArea.Multiple);

    public async Task<ComponentResult> GetDeleteFileResultAsync(string fileName)
    {
        await FormFileManager.DeleteFileAsync(fileName);
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
        FormFileManager.DeleteAllAsync();

    public Task<List<FormFileInfo>> GetFilesAsync() => 
        FormFileManager.GetFilesAsync();

    public Task ClearTemporaryFilesAsync() => 
        FormFileManager.DeleteAllAsync();

    public Task PromoteTemporaryFilesAsync(string folderKey) =>
        FormFileManager.PromoteTemporaryFilesAsync(folderKey);

    public async Task<FileComponentResult> GetDownloadFileResultAsync(string fileName)
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
        downloader.FileReference = await FormFileManager.GetFileReferenceAsync(fileName);
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
