using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Representa um form responsável por gerenciar arquivos de um diretório
/// </summary>
/// <example>
/// Exemplo de como utilizar JJFormUpload
/// [!code-cshtml[Example](../../../doc/JJMasterData.Sample/JJFormUploadExample.aspx)]
/// [!code-cs[Example](../../../doc/JJMasterData.Sample/JJFormUploadExample.aspx.cs)]
/// O Resultado html ficará parecido com esse:
/// <img src="../media/JJFormUploadFileExample.png"/>
/// </example>
/// <seealso cref="JJUploadFile"/>
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
    private JJUploadFile _upload;
    private FormFileService _service;

    /// <summary>
    /// Evento disparado ao incluir um novo arquivo
    /// </summary> 
    /// <remarks>
    /// O evento sempre é disparado antes de incluir o arquivo.
    /// Para realizar validações, basta retornar a mensagem de erro no retorno da função. 
    /// Se a função retornar diferente de nulo o conteúdo será exibido como erro
    /// e o evendo será cancelado.
    /// </remarks>
    public event EventHandler<FormUploadFileEventArgs> OnCreateFile;

    /// <summary>
    /// Evento disparado ao excluir um  arquivo
    /// </summary> 
    /// <remarks>
    /// O evento sempre é disparado antes de excluir o arquivo.
    /// Para realizar validações, basta retornar a mensagem de erro no retorno da função. 
    /// Se a função retornar diferente de nulo o conteúdo será exibido como erro
    /// e o evendo será cancelado.
    /// </remarks>
    public event EventHandler<FormDeleteFileEventArgs> OnDeleteFile;

    /// <summary>
    /// Evento disparado ao baixar o arquivo
    /// </summary> 
    /// <remarks>
    /// O evento sempre é disparado antes de baixar o arquivo.
    /// Para realizar validações, basta retornar a mensagem de erro no retorno da função. 
    /// Se a função retornar diferente de nulo o conteúdo será exibido como erro
    /// e o evendo será cancelado.
    /// </remarks>
    public event EventHandler<FormDownloadFileEventArgs> OnDownloadFile;

    /// <summary>
    /// Evento disparado ao renomear um arquivo
    /// </summary> 
    /// <remarks>
    /// O evento sempre é disparado antes de renomear o arquivo.
    /// Para realizar validações, basta retornar a mensagem de erro no retorno da função. 
    /// Se a função retornar diferente de nulo o conteúdo será exibido como erro
    /// e o evendo será cancelado.
    /// </remarks>
    public event EventHandler<FormRenameFileEventArgs> OnRenameFile;

    /// <summary>
    /// Exibe o painel para realizar upload de arquivos.
    /// Default: true
    /// </summary>
    public bool ShowAddFile { get; set; }

    /// <summary>
    /// Expandir ou recolher painel de upload de arquivos.
    /// Default: true (painel aberto)
    /// </summary>
    protected bool CollapseAriaExpanded { get; set; }

    /// <summary>
    /// Titulo do formulário.
    /// Default: Upload de Arquivos
    /// </summary>
    public string Title { get; set; }


    /// <summary>
    /// Sub-Titulo do formulário
    /// </summary>
    public string SubTitle { get; set; }

    public bool ViewGallery { get; set; }

    public FormFileService Service
    {
        get
        {
            if (_service == null)
            {
                _service = new FormFileService();
                _service.MemoryFilesSessionName = $"{Name}_jjfiles";
            }
            return _service;
        }
    }

    /// <summary>
    /// Objeto responsável por realizar upload dos arquivos
    /// </summary>
    public JJUploadFile Upload
    {
        get
        {
            if (_upload == null)
                _upload = new JJUploadFile();

            return _upload;
        }
    }

    /// <summary>
    /// Componente GridView
    /// </summary>
    public JJGridView GridView
    {
        get
        {
            if (_gridView != null)
                return _gridView;

            _gridView = new JJGridView
            {
                Name = Name + "_gridview",
                DataAccess = DataAccess,
                UserValues = UserValues,
                ShowPagging = false,
                ShowTitle = false
            };

            _gridView.FilterAction.SetVisible(false);
            _gridView.EmptyDataText = "There is no file to display";
            _gridView.ShowHeaderWhenEmpty = false;

            _gridView.AddGridAction(DownloadAction);
            _gridView.AddGridAction(RenameAction);
            _gridView.AddGridAction(DeleteAction);
            return _gridView;
        }
    }

    /// <summary>
    /// Ação da grid responsável por realizar o upload do arquivo
    /// </summary>
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

    /// <summary>
    /// Ação da grid responsável por realizar a exclusão do arquivo
    /// </summary>
    public ScriptAction DeleteAction
    {
        get
        {
            if (_deleteAction != null)
                return _deleteAction;

            string promptStr = Translate.Key("Would you like to delete this record?");
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

    /// <summary>
    /// Ação da grid responsável por renomear um arquivo
    /// </summary>
    public ScriptAction RenameAction
    {
        get
        {
            if (_renameAction != null)
                return _renameAction;

            string promptStr = Translate.Key("Enter the new name for the file:");
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


    public JJFormUpload()
    {
        ShowAddFile = true;
        Name = "jjuploadform1";
        CollapseAriaExpanded = true;
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
        string fileName = Cript.Descript64(previewVideo);
        var video = Service.GetFile(fileName);

        string srcVideo = "data:video/mp4;base64," +
                          Convert.ToBase64String(video.FileStream.ToArray(), 0, video.FileStream.ToArray().Length);


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
        string fileName = Cript.Descript64(previewImage);
        var file = Service.GetFile(fileName);

        if (file == null)
            return null;

        string src;
        if (file.IsInMemory)
        {
            src = $"data:image/{Path.GetExtension(fileName).Replace(".", "")};base64,{Convert.ToBase64String(file.FileStream.ToArray())}";
        }
        else
        {
            var filePath = Path.Combine(Service.FolderPath, fileName);
            var appPath = HttpContext.Current!.Request.ApplicationPath;

            if (!appPath.EndsWith("/"))
                appPath += "/";

            var culture = CultureInfo.CurrentCulture.Name + "/";
            src = $"{appPath}{culture}MasterData/Form/Download?filePath={Cript.Cript64(filePath)}".Trim();
        }

        var script = new StringBuilder();
        script.AppendLine("	$(document).ready(function () { ");
        script.AppendLine("   $('#img').css('max-height',window.innerHeight);");
        script.AppendLine("   $('#img').show('slow');");
        script.AppendLine("	}); ");

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
            {
                if (OnDeleteFile != null)
                {
                    var args = new FormDeleteFileEventArgs(fileName);
                    OnDeleteFile.Invoke(this, args);

                    if (!string.IsNullOrEmpty(args.ErrorMessage))
                        throw new Exception(args.ErrorMessage);
                }

                Service.DeleteFile(fileName);
            }
            else if ("DOWNLOADFILE".Equals(uploadAction))
            {
                if (OnDownloadFile != null)
                {
                    var args = new FormDownloadFileEventArgs(fileName, null);
                    OnDownloadFile.Invoke(this, args);

                    if (!string.IsNullOrEmpty(args.ErrorMessage))
                        throw new Exception(args.ErrorMessage);
                }

                DownloadFile(Path.Combine(Service.FolderPath, fileName));
            }
            else if ("RENAMEFILE".Equals(uploadAction))
            {
                string[] names = fileName.Split(';');
                string currentName = names[0];
                string newName = names[1];

                if (OnRenameFile != null)
                {
                    var args = new FormRenameFileEventArgs(currentName, newName);
                    OnRenameFile.Invoke(this, args);

                    if (!string.IsNullOrEmpty(args.ErrorMessage))
                        throw new Exception(args.ErrorMessage);
                }

                Service.RenameFile(currentName, newName);
            }
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

        html.AppendElement(new JJCollapsePanel
        {
            Title = "New File",
            ExpandedByDefault = CollapseAriaExpanded,
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
                Text = $"{Translate.Key("File Type:")}&nbsp;<b>{Upload.AllowedTypes}</b>"
            });
        }

        if (!Upload.Multiple && Service.CountFiles() > 0)
            Upload.AddLabel = Translate.Key("Update");

        panelContent.AppendElement(Upload);
        return panelContent;
    }

    private HtmlBuilder GetHtmlGridView()
    {
        if (GridView.DataSource == null &&
            GridView.FormElement == null)
        {
            var dt = GetDataTableFiles();

            if (dt == null) return GridView.GetHtmlBuilder();

            GridView.FormElement = new FormElement(dt);
            GridView.DataSource = dt;
            GridView.FormElement.Title = Title;
            GridView.FormElement.SubTitle = SubTitle;
            GridView.FormElement.Fields["NameJS"].VisibleExpression = "val:0";

            if (GridView.FormElement.Fields.Contains("LastWriteTime"))
                GridView.FormElement.Fields["LastWriteTime"].Label = "Last Modified";
        }

        return GridView.GetHtmlBuilder();
    }

    private HtmlBuilder GetHtmlGallery()
    {
        var files = Service.GetFiles();
        if (files.Count <= 0) return null;

        foreach (var ac in GridView.GridActions)
        {
            ac.IsGroup = false;
        }

        var row = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row");

        foreach (var file in files)
        {
            var col = new HtmlBuilder(HtmlTag.Div);
            col.WithCssClass("col-sm-3");
            col.AppendElement(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("list-group list-group-flush");
                ul.AppendElement(GetHtmlGalleryPreview(file.FileName));
                ul.AppendElement(GetHtmlGalleryListItem("Name", file.FileName));
                ul.AppendElement(GetHtmlGalleryListItem("Size", file.SizeBytes + " Bytes"));
                ul.AppendElement(GetHtmlGalleryListItem("Last Modified", file.LastWriteTime.ToString(CultureInfo.CurrentCulture)));
                ul.AppendElement(HtmlTag.Li, li =>
                {
                    li.WithCssClass("list-group-item");
                    li.AppendElement(HtmlTag.Table, table =>
                    {
                        table.WithCssClass("table-gallery");
                        table.AppendRange(GridView.Table.Body.GetActionsHtmlList(ConvertToHashtable(file)).ToList());
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
                b.AppendText(Translate.Key(label));
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
        var file = Service.GetFile(fileName);
        var url = CurrentContext.Request.AbsoluteUri;

        string src;
        string filePath = Path.Combine(Service.FolderPath, fileName);

        if (file.IsInMemory)
        {
            src = $"data:image/{Path.GetExtension(fileName).Replace(".", "")};base64,{Convert.ToBase64String(file.FileStream.ToArray())}";
        }
        else
        {
            var appPath = HttpContext.Current!.Request.ApplicationPath;

            if (!appPath.EndsWith("/"))
                appPath += "/";

            var culture = CultureInfo.CurrentCulture.Name + "/";
            src = $"{appPath}{culture}MasterData/Form/Download?filePath={Cript.Cript64(filePath)}".Trim();
        }

        if (url.Contains('?'))
            url += "&";
        else
            url += "?";

        url += "previewImage=";
        url += Cript.Cript64(fileName);

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
        videoUrl += Cript.Cript64(fileName);

        var html = new HtmlBuilder(HtmlTag.A)
         .WithAttribute("href", $"javascript:popup.show('{fileName}','{videoUrl}', 1);")
         .AppendElement(GetHtmlItemBox(fileName, "fa fa-play-circle", "red"));

        return html;
    }

    private Hashtable ConvertToHashtable(FileDetail file)
    {
        Hashtable hash = new();
        hash.Add(FileName, file.FileName);
        hash.Add(LastWriteTime, file.LastWriteTime);
        hash.Add(Size, file.SizeBytes);
        hash.Add(FileNameJs, file.FileName.Replace("'", "\\'"));

        return hash;
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
                   .AppendElement(new JJTextGroup
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
                    img.WithAttribute("id", $"pastedimage_{0}")
                       .WithAttribute("style", "max-height:350px;")
                       .WithAttribute("alt", Translate.Key("Preview Image"))
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

    private void UploadOnPostFile(object sender, FormUploadFileEventArgs e)
    {
        if (OnCreateFile != null)
        {
            var args = new FormUploadFileEventArgs(e.File);
            OnCreateFile.Invoke(this, args);
            string errorMessage = args.ErrorMessage;

            if (!string.IsNullOrEmpty(errorMessage))
            {
                e.ErrorMessage = errorMessage;
            }
        }

        try
        {
            if (!Upload.Multiple && Service.CountFiles() > 0)
            {
                foreach (var file in Service.GetFiles())
                {
                    Service.DeleteFile(file.FileName);
                }
            }

#if NETFRAMEWORK
            var stream = new MemoryStream();
            e.File.FileData.InputStream.CopyTo(stream);
            Service.CreateFile(e.File.FileData.FileName, stream);
#else
            using var stream = new MemoryStream();
            e.File.FileData.CopyTo(stream);
            Service.CreateFile(e.File.FileData.FileName, stream);
#endif


        }
        catch (Exception ex)
        {
            e.ErrorMessage = ex.Message;
        }

    }


    /// <summary>
    /// Recovers the list of files in a DataTable object.
    /// </summary>
    /// <returns>
    /// A data table with the following columns:<para></para>
    /// Name, Size, LastWriteTime
    /// </returns>
    public DataTable GetDataTableFiles()
    {
        var files = Service.GetFiles();
        var dt = new DataTable();
        dt.Columns.Add(FileName, typeof(string));
        dt.Columns.Add(Size, typeof(string));
        dt.Columns.Add(LastWriteTime, typeof(string));
        dt.Columns.Add(FileNameJs, typeof(string));

        foreach (var mFiles in files.Where(mFiles => !mFiles.Deleted))
        {
            var dataRow = dt.NewRow();
            dataRow["Name"] = mFiles.FileName;
            dataRow["Size"] = Format.FormatFileSize(mFiles.SizeBytes);
            dataRow["LastWriteTime"] = mFiles.LastWriteTime.ToDateTimeString();
            dataRow["NameJS"] = mFiles.FileName.Replace("'", "\\'");
            dt.Rows.Add(dataRow);
        }

        return dt;
    }

    public void DownloadFile(string fileName)
    {
        var download = new JJDownloadFile(fileName);

        download.ResponseDirectDownload();
    }

    /// <summary>
    /// Disable all actions, except the download.
    /// </summary>
    public void Disable()
    {
        ShowAddFile = false;
        foreach (BasicAction ac in GridView.GridActions)
        {
            ac.SetVisible(false);
        }
        DownloadAction.SetVisible(true);
    }

   

}
