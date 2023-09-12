using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.GridView;
using JJMasterData.Core.UI.Components.IO.UploadView;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Components;

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

    private ScriptAction _downloadAction;
    private ScriptAction _deleteAction;
    private ScriptAction _renameAction;
    private JJGridView _gridView;
    private JJUploadArea _uploadArea;
    private FormFileManager _formFileManager;
    private UploadViewScripts _scripts;
    
    public event EventHandler<FormUploadFileEventArgs> OnBeforeCreateFile;
    public event EventHandler<FormDeleteFileEventArgs> OnBeforeDeleteFile;
    public event EventHandler<FormRenameFileEventArgs> OnBeforeRenameFile;
    public event EventHandler<FormDownloadFileEventArgs> OnBeforeDownloadFile;
    
    public bool ShowAddFiles { get; set; }
    
    public bool IsCollapseExpandedByDefault { get; set; }
    
    public string Title { get; set; }
    public string SubTitle { get; set; }
    
    public bool ViewGallery { get; set; }

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
    
    public string JsCallback { get; set; } = @"document.forms[0].submit()";

    public JJGridView GridView
    {
        get
        {
            var files = GetFilesDataTable();
            if (_gridView != null)
            {
 
                _gridView.DataSource = EnumerableHelper.ConvertToDictionaryList(files);
                _gridView.TotalOfRecords = files.Rows.Count;
                return _gridView;
            }
            
            _gridView = ComponentFactory.GridView.Create(new FormElement(files));
            _gridView.FormElement.Name = Name;
            _gridView.FormElement.Title = Title;
            _gridView.FormElement.SubTitle = SubTitle;
            
            if(_gridView.FormElement.Fields.Contains("NameJS"))
                _gridView.FormElement.Fields["NameJS"].VisibleExpression = "val:0";

            if (_gridView.FormElement.Fields.Contains("LastWriteTime"))
                _gridView.FormElement.Fields["LastWriteTime"].Label = "Last Modified";
            
            _gridView.Name = $"{Name}-grid-view";
            _gridView.UserValues = UserValues;
            _gridView.ShowPagging = false;
            _gridView.ShowTitle = false;

            _gridView.FilterAction.SetVisible(false);
            _gridView.EmptyDataText = "There is no file to display";
            _gridView.ShowHeaderWhenEmpty = false;

            _gridView.GridActions.Clear();
            _gridView.AddGridAction(DownloadAction);

            _gridView.OnRenderAction += (_, args) =>
            {
                if(args.Action.Name.Equals(_downloadAction.Name))
                {
                    var fileName = args.FieldValues["Name"].ToString();
                    var isInMemory = FormFileManager.GetFile(fileName)?.IsInMemory ?? false;
                    if (isInMemory)
                    {
                        args.LinkButton.Enabled = false;
                    }
                }
            };

            _gridView.AddGridAction(RenameAction);
            _gridView.AddGridAction(DeleteAction);

            _gridView.FormElement.Options.Grid.TotalPerPage = int.MaxValue;
            _gridView.FormElement.Options.Grid.ShowPagging = false;
            return _gridView;
        }
    }
    
    public ScriptAction DownloadAction =>
        _downloadAction ??= new ScriptAction
        {
            Icon = IconType.CloudDownload,
            Tooltip = "Download File",
            Name = "download-file",
            OnClientClick = Scripts.GetDownloadFileScript()
        };

    public ScriptAction DeleteAction
    {
        get
        {
            if (_deleteAction != null)
                return _deleteAction;
            
            _deleteAction = new ScriptAction
            {
                Icon = IconType.Trash,
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
                Icon = IconType.PencilSquareO,
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

    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ILoggerFactory LoggerFactory { get; }
    private ILogger<JJUploadView> Logger { get; }
    public JJUploadView(
        IHttpContext currentContext,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
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
        
        var uploadAreaResult = await UploadArea.GetResultAsync();

        if (uploadAreaResult is JsonComponentResult)
        {
            return uploadAreaResult;
        }
        
        return await GetUploadViewResult();
    }

    public async Task<ComponentResult> GetUploadViewResult()
    {
        var previewImage = CurrentContext.Request.QueryString["previewImage"];
        if (!string.IsNullOrEmpty(previewImage))
            return HtmlComponentResult.FromHtmlBuilder(GetHtmlPreviewImage(previewImage));

        var previewVideo = CurrentContext.Request.QueryString["previewVideo"];
        if (!string.IsNullOrEmpty(previewVideo))
            return HtmlComponentResult.FromHtmlBuilder(GetHtmlPreviewVideo(previewVideo));

        var html = new HtmlBuilder();

        var uploadAction = CurrentContext.Request.GetFormValue($"upload-action-{Name}");
        if (!string.IsNullOrEmpty(uploadAction))
            GetUploadActionResult(uploadAction);

        if (!string.IsNullOrEmpty(Title))
            html.AppendComponent(new JJTitle(Title, SubTitle));

        html.Append(GetUploadAreaHtml());

        if (ViewGallery)
        {
            html.Append(await GetGalleryHtml());
        }
        else
        {
            html.Append(await GetGridViewHtml());
        }

        html.AppendComponent(await GetPreviewModalHtml());

        return new RenderedComponentResult(html);
    }

    private HtmlBuilder GetHtmlPreviewVideo(string previewVideo)
    {
        var fileName = EncryptionService.DecryptStringWithUrlUnescape(previewVideo);
        var video = FormFileManager.GetFile(fileName).Content;

        var srcVideo =
            $"data:video/mp4;base64,{Convert.ToBase64String(video.Bytes.ToArray(), 0, video.Bytes.ToArray().Length)}";
        
        var script = new StringBuilder();
        script.AppendLine("	$(document).ready(function () { ");
        script.AppendLine("   window.parent.$('#popup-modal').find('.close').click(function(){$('#video').trigger('pause')})");
        script.AppendLine("   $('#video').css('max-height',window.innerHeight);");
        script.AppendLine("	}); ");

        var html = new HtmlBuilder(HtmlTag.Div);
        html.Append(HtmlTag.Center, c =>
        {
            c.Append(HtmlTag.Video, video =>
            {
                video.WithAttribute("id", "video")
                     .WithAttribute("src", srcVideo)
                     .WithAttribute("autoplay", "autoplay")
                     .WithAttribute("style", "width:100%;height:100%;")
                     .WithCssClass("img-responsive");
            });
        });
        html.AppendScript(script.ToString());
        return html;
    }

    private HtmlBuilder GetHtmlPreviewImage(string previewImage)
    {
        var fileName = EncryptionService.DecryptStringWithUrlUnescape(previewImage);
        var file = FormFileManager.GetFile(fileName);

        if (file == null)
            return null;

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
            src = downloader.GetDownloadUrl(filePath);
        }

        const string script = """
            $(document).ready(function () {
                $('#img').css('max-height',window.innerHeight);
                $('#img').show('slow');
            });
        """;

        var html = new HtmlBuilder(HtmlTag.Div);
        html.Append(HtmlTag.Center, c =>
        {
            c.Append(HtmlTag.Img, img =>
            {
                img.WithAttribute("id", "img")
                   .WithAttribute("src", src)
                   .WithAttribute("alt", fileName)
                   .WithAttribute("style", "max-height:350px;display:none;")
                   .WithCssClass("img-responsive");
            });
        });
        html.AppendScript(script.ToString());
        return html;
    }

    private ComponentResult GetUploadActionResult(string uploadAction)
    {
        var fileName = CurrentContext.Request.GetFormValue($"filename-{Name}");
        try
        {
            if ("deleteFile".Equals(uploadAction))
                DeleteFile(fileName);
            else if ("downloadFile".Equals(uploadAction))
                DownloadFile(Path.Combine(FormFileManager.FolderPath, fileName));
            else if ("renameFile".Equals(uploadAction))
                RenameFile(fileName);
        }
        catch (Exception ex)
        {
            return new RenderedComponentResult(new JJMessageBox(ex.Message, MessageIcon.Warning).GetHtmlBuilder());
        }

        return new RenderedComponentResult(new JJMessageBox("Success", MessageIcon.Info).GetHtmlBuilder());
    }

    private HtmlBuilder GetUploadAreaHtml()
    {
        var html = new HtmlBuilder()
           .AppendHiddenInput($"upload-action-{Name}")
           .AppendHiddenInput($"filename-{Name}");

        if (!ShowAddFiles)
            return html;

        html.AppendComponent(new JJCollapsePanel(CurrentContext)
        {
            Title = "New File",
            ExpandedByDefault = IsCollapseExpandedByDefault,
            HtmlBuilderContent = GetHtmlFormPanel()
        });

        return html;
    }

    private HtmlBuilder GetHtmlFormPanel()
    {
        var panelContent = new HtmlBuilder();
        if (!UploadArea.AllowedTypes.Equals("*"))
        {
            panelContent.AppendComponent(new JJLabel
            {
                Text = $"{StringLocalizer["File Type:"]}&nbsp;<b>{UploadArea.AllowedTypes}</b>"
            });
        }

        if (!UploadArea.Multiple && FormFileManager.CountFiles() > 0)
            UploadArea.AddLabel = StringLocalizer["Update"];

        panelContent.Append(UploadArea.GetUploadAreaHtmlBuilder());
        return panelContent;
    }

    private async Task<HtmlBuilder> GetGridViewHtml()
    {
        return await GridView.GetHtmlBuilderAsync();
        
    }

    private async Task<HtmlBuilder> GetGalleryHtml()
    {
        var files = FormFileManager.GetFiles();
        if (files.Count <= 0) return null;

        foreach (var ac in GridView.GridActions)
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
            await col.AppendAsync(HtmlTag.Ul, async ul =>
            {
                ul.WithCssClass("list-group list-group-flush");
                ul.Append(GetHtmlGalleryPreview(file.FileName));
                ul.Append(GetHtmlGalleryListItem("Name", file.FileName));
                ul.Append(GetHtmlGalleryListItem("Size", $"{file.Length} Bytes"));
                ul.Append(GetHtmlGalleryListItem("Last Modified", file.LastWriteTime.ToString(CultureInfo.CurrentCulture)));
                await ul.AppendAsync(HtmlTag.Li, async li =>
                {
                    li.WithCssClass("list-group-item");
                    await li.AppendAsync(HtmlTag.Table, async table =>
                    {
                        table.WithCssClass("table-gallery");
                        var fileValues = ConvertFormFileToDictionary(file);
                        var formStateData = new FormStateData(fileValues, UserValues, PageState.List);
                        var htmlActions = GridView.Table.Body.GetActionsHtmlListAsync(formStateData);
                        await table.AppendRangeAsync(htmlActions);
                    });
                });
            });

            row.Append(col);
        }

        return row;
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
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-pdf-o", "red"));
                break;
            case ".pptx":
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-powerpoint-o", "red"));
                break;
            case ".docx":
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-word-o", "blue"));
                break;
            case ".csv":
            case ".txt":
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-text-o", "black"));
                break;
            case ".xls":
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-excel-o", "green"));
                break;
            case ".rar":
            case ".zip":
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-zip-o", "#d2bb1c"));
                break;
            default:
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.Append(GetHtmlItemBox(fileName, "fa fa-file-o", "gray"));
                break;
        }

        return html;
    }

    private static HtmlBuilder GetHtmlItemBox(string fileName, string cssIcon, string colorIcon)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("style", "height:180px;")
            .Append(HtmlTag.Span, span =>
            {
                span.WithCssClass(cssIcon)
                    .WithToolTip(fileName)
                    .WithAttribute("style", $"color:{colorIcon};padding-top:45px;font-size:100px;");
            });
        return div;
    }

    private HtmlBuilder GetHtmlImageBox(string fileName)
    {
        var file = FormFileManager.GetFile(fileName);
        var url = CurrentContext.Request.AbsoluteUri;

        string src;
        var filePath = Path.Combine(FormFileManager.FolderPath, fileName);

        if (file.IsInMemory)
        {
            var base64 = Convert.ToBase64String(file.Content.Bytes.ToArray());
            src = $"data:image/{Path.GetExtension(fileName).Replace(".", "")};base64,{base64}";
        }
        else
        {
            var downloader = ComponentFactory.Downloader.Create();
            src = downloader.GetDownloadUrl(filePath);
        }

        if (url.Contains('?'))
            url += "&";
        else
            url += "?";

        url += "previewImage=";
        url += EncryptionService.EncryptStringWithUrlEscape(fileName);

        var html = new HtmlBuilder(HtmlTag.A)
        .WithAttribute("href", $"javascript:defaultModal.showIframe('{url}', '{fileName}',1);")
        .Append(HtmlTag.Img, img =>
        {
            img.WithAttribute("loading", "lazy")
               .WithAttribute("src", src)
               .WithAttribute("style", "height:180px;")
               .WithCssClass("img-responsive")
               .WithToolTip(fileName);
        });

        return html;
    }

    private HtmlBuilder GetHtmlVideoBox(string fileName)
    {
        var videoUrl = CurrentContext.Request.AbsoluteUri;

        if (videoUrl.Contains('?'))
            videoUrl += "&";
        else
            videoUrl += "?";

        videoUrl += "previewVideo=";
        videoUrl += EncryptionService.EncryptStringWithUrlEscape(fileName);

        var html = new HtmlBuilder(HtmlTag.A)
         .WithAttribute("href", $"javascript:defaultModal.showIframe('{videoUrl}', '{fileName}',1);")
         .Append(GetHtmlItemBox(fileName, "fa fa-play-circle", "red"));

        return html;
    }

    private static IDictionary<string, object> ConvertFormFileToDictionary(FormFileContent file)
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

        await html.AppendAsync(HtmlTag.Div, async row =>
        {
            row.WithCssClass("row");
            await row.AppendAsync(HtmlTag.Div, async col =>
            {
                col.WithCssClass("col-sm-12")
                   .AppendComponent(new JJLabel
                   {
                       LabelFor = $"preview_filename-{UploadArea.Name}",
                       Text = "File name"
                   });
                   await col.AppendControlAsync(new JJTextGroup(CurrentContext)
                   {
                       Name = $"preview_filename-{UploadArea.Name}",
                       Addons = new InputAddons(".png"),
                       Text = "image"
                   });
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
                       .WithAttribute("style", "max-height:350px;")
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
            Text = "Save"
        };

        var btnCancel = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            Text = "Cancel"
        };
        btnCancel.SetAttr(BootstrapHelper.DataDismiss, "modal");

        var modal = new JJModalDialog
        {
            Name = $"preview_modal_{UploadArea.Name}",
            Title = "Would you like to save the image below?",
            HtmlBuilderContent = html
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
            dataRow["NameJS"] = content.FileName.Replace("'", "\\'");
            dt.Rows.Add(dataRow);
        }

        return dt;
    }

    private void OnFileUploaded(object sender, FormUploadFileEventArgs args)
    {
        try
        {
            CreateFile(args.File);
            args.SuccessMessage = "File sucessfully created.";
        }
        catch (Exception ex)
        {
            args.ErrorMessage = ex.Message;
        }
    }

    private void RenameFile(string fileName)
    {
        var names = fileName.Split(';');
        var currentName = names[0];
        var newName = names[1];
        RenameFile(currentName, newName);
    }

    public void RenameFile(string currentName, string newName) =>
      FormFileManager.RenameFile(currentName, newName);

    public void CreateFile(FormFileContent file) =>
        FormFileManager.CreateFile(file, !UploadArea.Multiple);

    public void DeleteFile(string fileName) =>
        FormFileManager.DeleteFile(fileName);

    internal void DeleteAll() => 
        FormFileManager.DeleteAll();

    public List<FormFileInfo> GetFiles() => 
        FormFileManager.GetFiles();

    public void ClearMemoryFiles() => 
        FormFileManager.MemoryFiles = null;

    public void SaveMemoryFiles(string folderPath) =>
        FormFileManager.SaveMemoryFiles(folderPath);

    public void DownloadFile(string fileName)
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
        downloader.GetDirectDownloadRedirect();
    }

    /// <summary>
    /// Disable all actions, except the download.
    /// </summary>
    public void Disable()
    {
        ShowAddFiles = false;
        foreach (var action in GridView.GridActions)
        {
            action.SetVisible(false);
        }
        DownloadAction.SetVisible(true);
    }

}
