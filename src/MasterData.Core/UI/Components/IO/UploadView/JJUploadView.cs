using System;
using System.Collections.Generic;
using System.Data;
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
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Tasks;
using JJMasterData.Core.UI.Components.CollapsePanel;
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
    private const string Size = "Size";
    private const string LastWriteTime = "LastWriteTime";

    private UrlRedirectAction _downloadAction;
    private ScriptAction _deleteAction;
    private ScriptAction _renameAction;
    private JJGridView _gridView;
    private JJUploadArea _uploadArea;
    private FormFileManager _formFileManager;
    private UploadViewScripts _scripts;
    private RouteContext _routeContext;
    
    public event EventHandler<FormUploadFileEventArgs> OnBeforeCreateFile;
    public event EventHandler<FormDeleteFileEventArgs> OnBeforeDeleteFile;
    public event EventHandler<FormRenameFileEventArgs> OnBeforeRenameFile;
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

    /// <summary>
    /// Full Directory Path.
    /// (Optional) If the path is not given, all files will be stored in the session.
    /// </summary>
    /// <remarks>
    /// Example: C:\temp\files\ (Windows) or /tmp/Files (Linux)
    /// </remarks>
    public string FolderPath { get; set; }

    public JJUploadArea UploadArea
    {
        get
        {
            if (_uploadArea != null)
                return _uploadArea;
            
            _uploadArea = ComponentFactory.UploadArea.Create();
            _uploadArea.OnFileUploaded += OnFileUploaded;
            _uploadArea.JsCallback = JsCallback;
            _uploadArea.Name = $"{Name}-files";

            return _uploadArea;
        }
    }
    
    public string JsCallback { get; set; } = "getMasterDataForm().submit()";

    public JJGridView GridView
    {
        get
        {
            var files = GetFilesDataTable();
            
            if (_gridView != null)
            {
                _gridView.DataSource = EnumerableHelper.ConvertToDictionaryList(files);
                return _gridView;
            }

            _gridView = ComponentFactory.GridView.Create(new FormElement(files));
            _gridView.FormElement.Name = Name;
            _gridView.FormElement.ParentName = ParentName;
            _gridView.FormElement.Title = Title;
            _gridView.FormElement.SubTitle = SubTitle;
            _gridView.FormElement.TitleSize = TitleSize;
            _gridView.FormElement.Icon = Icon;
            
            _gridView.DataSource = EnumerableHelper.ConvertToDictionaryList(files);
            _gridView.TotalOfRecords = files.Rows.Count;

            if(_gridView.FormElement.Fields.Contains("NameJS"))
                _gridView.FormElement.Fields["NameJS"].VisibleExpression = "val:0";

            if (_gridView.FormElement.Fields.Contains("LastWriteTime"))
                _gridView.FormElement.Fields["LastWriteTime"].Label = "Last Modified";
            
            _gridView.FormElement.Options.GridToolbarActions.InsertAction.SetVisible(false);
            
            _gridView.Name = $"{Name}-grid-view";
            _gridView.UserValues = UserValues;
            _gridView.ShowPaging = false;
            _gridView.ShowTitle = false;


            _gridView.FilterAction.SetVisible(false);
            _gridView.EmptyDataText = StringLocalizer["There is no files to display."];
            _gridView.ShowHeaderWhenEmpty = false;
            
            _gridView.ViewAction.SetVisible(false);
            _gridView.EditAction.SetVisible(false);
            _gridView.DeleteAction.SetVisible(false);
            _gridView.ExportAction.SetVisible(false);
            _gridView.RefreshAction.SetVisible(false);
            _gridView.ConfigAction.SetVisible(false);
            
            _gridView.TableActions.Add(DownloadAction);
            
            _gridView.OnRenderActionAsync += (_, args) =>
            {
                if(args.ActionName.Equals(_downloadAction.Name))
                {
                    var fileName = args.FieldValues["Name"].ToString();
                    var file = FormFileManager.GetFile(fileName);
                    var isInMemory = file?.IsInMemory ?? false;
                    var isRenamed = file?.IsRenamed ?? false;
                    if (isInMemory || isRenamed)
                    {
                        args.LinkButton.Enabled = false;
                        args.LinkButton.Tooltip = StringLocalizer["Save your form to download this file."];
                    }
                    else
                    {
                        var downloader = ComponentFactory.Downloader.Create();
                        downloader.FilePath = FormFileManager.GetFilePath(fileName);
                        args.LinkButton.UrlAction = downloader.GetDownloadUrl();
                    } 
                }

                return ValueTaskHelper.CompletedTask;
            };

            _gridView.TableActions.Add(RenameAction);
            _gridView.TableActions.Add(DeleteAction);
        
            
            _gridView.FormElement.Options.Grid.RecordsPerPage = int.MaxValue;
            _gridView.FormElement.Options.Grid.ShowPagging = false;
            _gridView.FormElement.Options.Grid.ShowToolBar = false;
            
            return _gridView;
        }
    }

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
                _formFileManager = new FormFileManager($"{Name}-files", CurrentContext,StringLocalizer, LoggerFactory.CreateLogger<FormFileManager>());
                _formFileManager.OnBeforeCreateFile += OnBeforeCreateFile;
                _formFileManager.OnBeforeDeleteFile += OnBeforeDeleteFile;
                _formFileManager.OnBeforeRenameFile += OnBeforeRenameFile;
            }
            _formFileManager.AutoSave = AutoSave;
            _formFileManager.FolderPath = FolderPath;
            return _formFileManager;
        }
    }

    private UploadViewScripts Scripts => _scripts ??= new UploadViewScripts(this);

    private IHttpContext CurrentContext { get; }
    private IComponentFactory ComponentFactory { get; }
    private IEncryptionService EncryptionService { get; }

    protected RouteContext RouteContext
    {
        get
        {
            if (_routeContext != null)
                return _routeContext;

            var factory = new RouteContextFactory(CurrentContext.Request.QueryString, EncryptionService);
            _routeContext = factory.Create();
            
            return _routeContext;
        }
    }

    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; }
    private ILoggerFactory LoggerFactory { get; }
    private ILogger<JJUploadView> Logger { get; }
    public JJUploadView(
        IHttpContext currentContext,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    {
        CurrentContext = currentContext;
        ComponentFactory = componentFactory;
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
            return downloader.GetDownloadResult();
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
        var html = new HtmlBuilder(HtmlTag.Div);

        var uploadAction = CurrentContext.Request.Form[$"upload-view-action-{Name}"];
        if (!string.IsNullOrEmpty(uploadAction))
        {
            var result = GetUploadActionResult(uploadAction);

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
            var result = await GetGridViewResult();

            if (result is RenderedComponentResult renderedComponent)
            {
                html.Append(renderedComponent.HtmlBuilder);
            }
            else
            {
                return result;
            }
        }

        html.AppendComponent(await GetPreviewModalHtml());

        html.WithId(Name);
        
        return new RenderedComponentResult(html);
    }

    private ComponentResult GetUploadActionResult(string uploadViewAction)
    {
        var fileName = CurrentContext.Request.Form[$"upload-view-file-name-{Name}"];
        try
        {
            switch (uploadViewAction)
            {
                case "deleteFile":
                    return GetDeleteFileResult(fileName);
                case "downloadFile":
                    return GetDownloadFileResult(Path.Combine(FormFileManager.FolderPath, fileName));
                case "renameFile":
                    return GetRenameFileResult(fileName);
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
           .AppendHiddenInput($"upload-view-file-name-{Name}");

        if (!ShowAddFiles)
            return html;

        html.AppendComponent(new JJMasterDataCollapsePanel(CurrentContext.Request.Form)
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

    private Task<ComponentResult> GetGridViewResult()
    {
        return GridView.GetResultAsync();
        
    }

    private async Task<HtmlBuilder> GetGalleryHtml()
    {
        var files = GetFiles().FindAll(x => !x.Deleted);
        if (files.Count == 0) 
            return new JJAlert{Title = StringLocalizer["There is no files to display."]}.GetHtmlBuilder();

        foreach (var ac in GridView.TableActions)
        {
            ac.IsGroup = false;
        }

        var row = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row");

        foreach (var fileInfo in files)
        {
            var file = fileInfo.Content;
            var col = new HtmlBuilder(HtmlTag.Div);
            col.WithCssClass("col-sm-3");
            
            var fileValues = ConvertFormFileToDictionary(file);
            var formStateData = new FormStateData(fileValues, UserValues, PageState.List);
            var actionsHtml = await GridView.Table.Body.GetActionsHtmlListAsync(formStateData); 
            
            col.Append(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("list-group list-group-flush");
                ul.Append(GetHtmlGalleryPreview(file.FileName));
                
                ul.AppendRange(GetGalleryListItems(file));
                
                ul.Append(HtmlTag.Li, li =>
                {
                    li.WithCssClass("list-group-item");
                    li.Append(HtmlTag.Table, table =>
                    {
                        table.WithCssClass("table-gallery");
                        table.AppendRange(actionsHtml);
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

    private HtmlBuilder GetHtmlGalleryPreview(string fileName)
    {
        var html = new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("list-group-item");

        switch (Path.GetExtension(fileName))
        {
            case ".png":
            case ".jpg":
            case ".jpeg":
                html.Append(GetHtmlImageBox(fileName));
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

    private HtmlBuilder GetHtmlImageBox(string fileName)
    {
        var file = FormFileManager.GetFile(fileName);

        string src;
        
        if (file.IsInMemory)
        {
            var base64 = Convert.ToBase64String(file.Content.Bytes.ToArray());
            src = $"data:image/{Path.GetExtension(fileName).Replace(".", "")};base64,{base64}";
        }
        else
        {
            var filePath = Path.Combine(FormFileManager.FolderPath, fileName);
            var downloader = ComponentFactory.Downloader.Create();
            downloader.FilePath = filePath;
            src = downloader.GetDownloadUrl();
        }
        
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

    private static Dictionary<string, object> ConvertFormFileToDictionary(FormFileContent file)
    {
        var dictionary = new Dictionary<string, object>
        {
            { FileName, file.FileName },
            { LastWriteTime, file.LastWriteTime },
            { Size, file.Length },
            { FileNameJs, file.FileName.Replace("'", "\\'") }
        };

        return dictionary;
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

    private DataTable GetFilesDataTable()
    {
        var files = FormFileManager.GetFiles();
        var dt = new DataTable();
        dt.Columns.Add(FileName, typeof(string));
        dt.Columns.Add(Size, typeof(string));
        dt.Columns.Add(LastWriteTime, typeof(string));
        dt.Columns.Add(FileNameJs, typeof(string));

        foreach (var fileInfo in files.Where(f => !f.Deleted))
        {
            var content = fileInfo.Content;
            var dataRow = dt.NewRow();
            dataRow["Name"] = content.FileName;
            dataRow["Size"] = Format.FormatFileSize(content.Length);
            dataRow["LastWriteTime"] = content.LastWriteTime.ToDateTimeString();
            dataRow["NameJS"] = HttpUtility.JavaScriptStringEncode(content.FileName);
            dt.Rows.Add(dataRow);
        }

        return dt;
    }

    private void OnFileUploaded(object sender, FormUploadFileEventArgs args)
    {
        try
        {
            CreateFile(args.File);
            args.SuccessMessage = "File successfully created.";
        }
        catch (Exception ex)
        {
            args.ErrorMessage = ex.Message;
        }
    }

    private RenderedComponentResult GetRenameFileResult(string fileName)
    {
        var names = fileName.Split(';');
        var currentName = names[0];
        var newName = names[1];
        RenameFile(currentName, newName);

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

    public void RenameFile(string currentName, string newName) =>
      FormFileManager.RenameFile(currentName, newName);

    public void CreateFile(FormFileContent file) =>
        FormFileManager.CreateFile(file, !UploadArea.Multiple);

    public ComponentResult GetDeleteFileResult(string fileName)
    {
        FormFileManager.DeleteFile(fileName);
        var text = StringLocalizer["File successfully deleted."];
        var alert = new JJAlert();
        alert.Title = text;
        alert.ShowCloseButton = true;
        alert.Color = BootstrapColor.Info;
        alert.Icon = FontAwesomeIcon.SolidCircleInfo;
        
        return new RenderedComponentResult(alert.GetHtmlBuilder());
    }
    
    internal void DeleteAll() => 
        FormFileManager.DeleteAll();

    public List<FormFileInfo> GetFiles() => 
        FormFileManager.GetFiles();

    public void ClearMemoryFiles() => 
        FormFileManager.MemoryFiles = null;

    public void SaveMemoryFiles(string folderPath) =>
        FormFileManager.SaveMemoryFiles(folderPath);

    public FileComponentResult GetDownloadFileResult(string fileName)
    {
        if (OnBeforeDownloadFile != null)
        {
            var args = new FormDownloadFileEventArgs(fileName, null);
            OnBeforeDownloadFile.Invoke(this, args);

            
            if (!string.IsNullOrEmpty(args.ErrorMessage))
            {
                var exception = new JJMasterDataException(args.ErrorMessage);
                Logger.LogError(exception, "Error on OnBeforeDownloadFile event");
                throw exception;
            }
        }
        var downloader = ComponentFactory.Downloader.Create();
        downloader.FilePath = fileName;
        return downloader.GetDownloadResult();
    }

    /// <summary>
    /// Disable all actions, except the download.
    /// </summary>
    public void Disable()
    {
        ShowAddFiles = false;
        foreach (var action in GridView.TableActions)
        {
            action.SetVisible(false);
        }
        DownloadAction.SetVisible(true);
    }

}
