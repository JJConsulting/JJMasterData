using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Cryptography;
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
/// TODO: New name suggestion: JJUploadView (I think we should use view for components with other components inside)
public class JJFormUpload : JJBaseView
{
    private const string FileName = "Name";
    private const string FileNameJs = "NameJS";
    private const string Size = "Size";
    private const string LastWriteTime = "LastWriteTime";

    private ScriptAction _downloadAction;
    private ScriptAction _deleteAction;
    private ScriptAction _renameAction;
    private JJGridView _gridView;
    private JJUploadArea _upload;
    private FormFileManager _service;

    public event EventHandler<FormUploadFileEventArgs> OnBeforeCreateFile;
    public event EventHandler<FormDeleteFileEventArgs> OnBeforeDeleteFile;
    public event EventHandler<FormRenameFileEventArgs> OnBeforeRenameFile;
    public event EventHandler<FormDownloadFileEventArgs> OnBeforeDownloadFile;
    
    /// <summary>
    /// Render upload colapse panel 
    /// (default is true)
    /// </summary>
    public bool ShowAddFile { get; set; }

    /// <summary>
    /// Colapse panel uplod is expanded 
    /// (default is true)
    /// </summary>
    public bool ExpandedByDefault { get; set; }

    /// <summary>
    /// Form Tile
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Form Sub-Tile
    /// </summary>
    public string SubTitle { get; set; }

    /// <summary>
    /// Show images as gallery
    /// </summary>
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
    /// Example: c:\temp\files\
    /// </remarks>
    public string FolderPath { get; set; }

    public JJUploadArea Upload => _upload ??= ComponentFactory.UploadArea.Create();

    public JJGridView GridView
    {
        get
        {
            if (_gridView != null)
                return _gridView;

            var dt = GetDataTableFiles();

            _gridView = ComponentFactory.GridView.Create(new FormElement(dt));
            _gridView.DataSource = dt;
            _gridView.FormElement.Title = Title;
            _gridView.FormElement.SubTitle = SubTitle;
            
            if(_gridView.FormElement.Fields.Contains("NameJS"))
                _gridView.FormElement.Fields["NameJS"].VisibleExpression = "val:0";

            if (_gridView.FormElement.Fields.Contains("LastWriteTime"))
                _gridView.FormElement.Fields["LastWriteTime"].Label = "Last Modified";
            
            _gridView.Name = Name + "_gridview";
            _gridView.UserValues = UserValues;
            _gridView.ShowPagging = false;
            _gridView.ShowTitle = false;

            _gridView.FilterAction.SetVisible(false);
            _gridView.EmptyDataText = "There is no file to display";
            _gridView.ShowHeaderWhenEmpty = false;

            _gridView.AddGridAction(DownloadAction);

            _gridView.OnRenderAction += (object sender, ActionEventArgs args) =>
            {
                if(args.Action.Name.Equals(_downloadAction.Name))
                {
                    var fileName = args.FieldValues["Name"].ToString();
                    var isInMemory = FormFileManager.GetFile(fileName).IsInMemory;
                    if (isInMemory)
                    {
                        args.LinkButton.Enabled = false;
                    }
                }
            };

            _gridView.AddGridAction(RenameAction);
            _gridView.AddGridAction(DeleteAction);
            return _gridView;
        }
    }
    
    public ScriptAction DownloadAction
    {
        get
        {
            if (_downloadAction == null)
                _downloadAction = new ScriptAction
                {
                    Icon = IconType.CloudDownload,
                    ToolTip = "Download File",
                    Name = "DOWNLOADFILE",
                    OnClientClick = "jjview.downloadFile('" + Name + "','{NameJS}');"
                };

            return _downloadAction;
        }
    }
    
    public ScriptAction DeleteAction
    {
        get
        {
            if (_deleteAction != null)
                return _deleteAction;

            string promptStr = StringLocalizer["Would you like to delete this record?"];
            _deleteAction = new ScriptAction
            {
                Icon = IconType.Trash,
                ToolTip = "Delete File",
                OnClientClick = "jjview.deleteFile('" + Name + "','{NameJS}', '" + promptStr + "');",
                Name = "DELFILE"
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

            string promptStr = StringLocalizer["Enter the new name for the file:"];
            _renameAction = new ScriptAction
            {
                Icon = IconType.PencilSquareO,
                ToolTip = "Rename File",
                OnClientClick = "jjview.renameFile('" + Name + "','{NameJS}','" + promptStr + "');",
                Name = "RENAMEFILE"
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
            if (_service == null)
            {
                _service = new FormFileManager(Name, CurrentContext,StringLocalizer, LoggerFactory.CreateLogger<FormFileManager>());
                _service.OnBeforeCreateFile += OnBeforeCreateFile;
                _service.OnBeforeDeleteFile += OnBeforeDeleteFile;
                _service.OnBeforeRenameFile += OnBeforeRenameFile;
            }
            _service.AutoSave = AutoSave;
            _service.FolderPath = FolderPath;
            return _service;
        }
    }

    internal IHttpContext CurrentContext { get; }
    private ComponentFactory ComponentFactory { get; }
    internal JJMasterDataEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ILoggerFactory LoggerFactory { get; }
    internal ILogger<JJFormUpload> Logger { get; }
    public JJFormUpload(
        IHttpContext currentContext,
        ComponentFactory componentFactory,
        JJMasterDataEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    {
        CurrentContext = currentContext;
        ComponentFactory = componentFactory;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
        Logger = loggerFactory.CreateLogger<JJFormUpload>();
        Name = "jjuploadform1";
        ShowAddFile = true;
        ExpandedByDefault = true;
    }

    internal override HtmlBuilder RenderHtml()
    {
        Upload.OnPostFile += UploadOnPostFile;
        string previewImage = CurrentContext.Request["previewImage"];
        if (!string.IsNullOrEmpty(previewImage))
            return GetHtmlPreviewImage(previewImage);

        string previewVideo = CurrentContext.Request["previewVideo"];
        if (!string.IsNullOrEmpty(previewVideo))
            return GetHtmlPreviewVideo(previewVideo);

        var html = new HtmlBuilder();

        string uploadAction = CurrentContext.Request["uploadaction_" + Name];
        if (!string.IsNullOrEmpty(uploadAction))
            html.AppendElement(GetResponseAction(uploadAction));

        if (!string.IsNullOrEmpty(Title))
            html.AppendElement(new JJTitle(Title, SubTitle));

        html.AppendElement(GetHtmlForm());
        html.AppendElement(ViewGallery ? GetHtmlGallery() : GetHtmlGridView());
        html.AppendElement(GetHtmlPreviewModal());

        return html;
    }

    private HtmlBuilder GetHtmlPreviewVideo(string previewVideo)
    {
        string fileName = EncryptionService.DecryptStringWithUrlDecode(previewVideo);
        var video = FormFileManager.GetFile(fileName).Content;

        string srcVideo = "data:video/mp4;base64," +
                          Convert.ToBase64String(video.Bytes.ToArray(), 0, video.Bytes.ToArray().Length);


        var script = new StringBuilder();
        script.AppendLine("	$(document).ready(function () { ");
        script.AppendLine("   window.parent.$('#popup-modal').find('.close').click(function(){$('#video').trigger('pause')})");
        script.AppendLine("   $('#video').css('max-height',window.innerHeight);");
        script.AppendLine("	}); ");

        var html = new HtmlBuilder(HtmlTag.Div);
        html.AppendElement(HtmlTag.Center, c =>
        {
            c.AppendElement(HtmlTag.Video, video =>
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
        string fileName = EncryptionService.DecryptStringWithUrlDecode(previewImage);
        var file = FormFileManager.GetFile(fileName);

        if (file == null)
            return null;

        string src;
        if (file.IsInMemory)
        {
            string base64 = Convert.ToBase64String(file.Content.Bytes.ToArray());
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
        html.AppendElement(HtmlTag.Center, c =>
        {
            c.AppendElement(HtmlTag.Img, img =>
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

    private HtmlBuilder GetResponseAction(string uploadAction)
    {
        string fileName = CurrentContext.Request.Form("filename_" + Name);
        try
        {
            if ("DELFILE".Equals(uploadAction))
                DeleteFile(fileName);
            else if ("DOWNLOADFILE".Equals(uploadAction))
                DownloadFile(Path.Combine(FormFileManager.FolderPath, fileName));
            else if ("RENAMEFILE".Equals(uploadAction))
                RenameFile(fileName);
        }
        catch (Exception ex)
        {
            return new JJMessageBox(ex.Message, MessageIcon.Warning).GetHtmlBuilder();
        }

        return null;
    }

    private HtmlBuilder GetHtmlForm()
    {
        var html = new HtmlBuilder()
           .AppendHiddenInput($"uploadaction_{Name}")
           .AppendHiddenInput($"filename_{Name}");

        if (!ShowAddFile)
            return html;

        html.AppendElement(new JJCollapsePanel(CurrentContext)
        {
            Title = "New File",
            ExpandedByDefault = ExpandedByDefault,
            HtmlBuilderContent = GetHtmlFormPanel()
        });

        return html;
    }

    private HtmlBuilder GetHtmlFormPanel()
    {
        var panelContent = new HtmlBuilder();
        if (!Upload.AllowedTypes.Equals("*"))
        {
            panelContent.AppendElement(new JJLabel
            {
                Text = $"{StringLocalizer["File Type:"]}&nbsp;<b>{Upload.AllowedTypes}</b>"
            });
        }

        if (!Upload.Multiple && FormFileManager.CountFiles() > 0)
            Upload.AddLabel = StringLocalizer["Update"];

        panelContent.AppendElement(Upload);
        return panelContent;
    }

    private HtmlBuilder GetHtmlGridView()
    {
        return GridView.GetHtmlBuilder();
    }

    private HtmlBuilder GetHtmlGallery()
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
            col.AppendElement(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("list-group list-group-flush");
                ul.AppendElement(GetHtmlGalleryPreview(file.FileName));
                ul.AppendElement(GetHtmlGalleryListItem("Name", file.FileName));
                ul.AppendElement(GetHtmlGalleryListItem("Size", file.Length + " Bytes"));
                ul.AppendElement(GetHtmlGalleryListItem("Last Modified", file.LastWriteTime.ToString(CultureInfo.CurrentCulture)));
                ul.AppendElement(HtmlTag.Li, li =>
                {
                    li.WithCssClass("list-group-item");
                    li.AppendElement(HtmlTag.Table, table =>
                    {
                        table.WithCssClass("table-gallery");
                        table.AppendRange(GridView.Table.Body.GetActionsHtmlList(ConvertFormFileToDictionary(file)).ToList());
                    });
                });
            });

            row.AppendElement(col);
        }

        return row;
    }

    private HtmlBuilder GetHtmlGalleryListItem(string label, string value)
    {
        return new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("list-group-item")
            .AppendElement(HtmlTag.B, b =>
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
                html.AppendElement(GetHtmlImageBox(fileName));
                break;
            case ".mp4":
                html.WithCssClass("text-center");
                html.AppendElement(GetHtmlVideoBox(fileName));
                break;
            case ".pdf":
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.AppendElement(GetHtmlItemBox(fileName, "fa fa-file-pdf-o", "red"));
                break;
            case ".pptx":
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.AppendElement(GetHtmlItemBox(fileName, "fa fa-file-powerpoint-o", "red"));
                break;
            case ".docx":
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.AppendElement(GetHtmlItemBox(fileName, "fa fa-file-word-o", "blue"));
                break;
            case ".csv":
            case ".txt":
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.AppendElement(GetHtmlItemBox(fileName, "fa fa-file-text-o", "black"));
                break;
            case ".xls":
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.AppendElement(GetHtmlItemBox(fileName, "fa fa-file-excel-o", "green"));
                break;
            case ".rar":
            case ".zip":
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.AppendElement(GetHtmlItemBox(fileName, "fa fa-file-zip-o", "#d2bb1c"));
                break;
            default:
                html.WithCssClass("text-center");
                html.WithAttribute("style", "background-color:#f5f5f5");
                html.AppendElement(GetHtmlItemBox(fileName, "fa fa-file-o", "gray"));
                break;
        }

        return html;
    }

    private HtmlBuilder GetHtmlItemBox(string fileName, string cssIcon, string colorIcon)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("style", "height:180px;")
            .AppendElement(HtmlTag.Span, span =>
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
        string filePath = Path.Combine(FormFileManager.FolderPath, fileName);

        if (file.IsInMemory)
        {
            string base64 = Convert.ToBase64String(file.Content.Bytes.ToArray());
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
        url += EncryptionService.EncryptStringWithUrlEncode(fileName);

        var html = new HtmlBuilder(HtmlTag.A)
        .WithAttribute("href", $"javascript:popup.show('{fileName}','{url}', 1);")
        .AppendElement(HtmlTag.Img, img =>
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
        string videoUrl = CurrentContext.Request.AbsoluteUri;

        if (videoUrl.Contains('?'))
            videoUrl += "&";
        else
            videoUrl += "?";

        videoUrl += "previewVideo=";
        videoUrl += EncryptionService.EncryptStringWithUrlEncode(fileName);

        var html = new HtmlBuilder(HtmlTag.A)
         .WithAttribute("href", $"javascript:popup.show('{fileName}','{videoUrl}', 1);")
         .AppendElement(GetHtmlItemBox(fileName, "fa fa-play-circle", "red"));

        return html;
    }

    private static IDictionary<string,dynamic> ConvertFormFileToDictionary(FormFileContent file)
    {
        var dictionary = new Dictionary<string,dynamic>
        {
            { FileName, file.FileName },
            { LastWriteTime, file.LastWriteTime },
            { Size, file.Length },
            { FileNameJs, file.FileName.Replace("'", "\\'") }
        };

        return dictionary;
    }

    private JJModalDialog GetHtmlPreviewModal()
    {
        var html = new HtmlBuilder(HtmlTag.Div);

        html.AppendElement(HtmlTag.Div, row =>
        {
            row.WithCssClass("row");
            row.AppendElement(HtmlTag.Div, col =>
            {
                col.WithCssClass("col-sm-12")
                   .AppendElement(new JJLabel
                   {
                       LabelFor = $"preview_filename_{Upload.Name}",
                       Text = "File name"
                   })
                   .AppendElement(new JJTextGroup(CurrentContext)
                   {
                       Name = $"preview_filename_{Upload.Name}",
                       Addons = new InputAddons(".png"),
                       Text = "image"
                   });
            });
        });

        html.AppendElement(HtmlTag.Div, row =>
        {
            row.WithCssClass("row");
            row.AppendElement(HtmlTag.Div, col =>
            {
                col.WithCssClass("col-sm-12");
                col.AppendElement(HtmlTag.Hr);
            });
            row.AppendElement(HtmlTag.Div, col =>
            {
                col.WithCssClass("col-sm-12");
                col.AppendElement(HtmlTag.Img, img =>
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
            Name = $"btnDoUpload_{Upload.Name}",
            CssClass = "btn btn-primary",
            Text = "Save"
        };

        var btnCancel = new JJLinkButton
        {
            Type = LinkButtonType.Button,
            Text = "Cancel"
        };
        btnCancel.SetAttr(BootstrapHelper.DataDismiss, "modal");

        var modal = new JJModalDialog();
        modal.Name = $"preview_modal_{Upload.Name}";
        modal.Title = "Would you like to save the image below?";
        modal.HtmlBuilderContent = html;
        modal.Buttons.Add(btnOk);
        modal.Buttons.Add(btnCancel);

        return modal;
    }

    private DataTable GetDataTableFiles()
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

    private void UploadOnPostFile(object sender, FormUploadFileEventArgs e)
    {
        try
        {
            CreateFile(e.File);
        }
        catch (Exception ex)
        {
            e.ErrorMessage = ex.Message;
        }
    }

    private void RenameFile(string fileName)
    {
        string[] names = fileName.Split(';');
        string currentName = names[0];
        string newName = names[1];
        RenameFile(currentName, newName);
    }

    public void RenameFile(string currentName, string newName) =>
      FormFileManager.RenameFile(currentName, newName);

    public void CreateFile(FormFileContent file) =>
        FormFileManager.CreateFile(file, !Upload.Multiple);

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
        downloader.DirectDownload();
    }

    /// <summary>
    /// Disable all actions, except the download.
    /// </summary>
    public void Disable()
    {
        ShowAddFile = false;
        foreach (var action in GridView.GridActions)
        {
            action.SetVisible(false);
        }
        DownloadAction.SetVisible(true);
    }

}
