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
using JJMasterData.Core.FormEvents.Args;
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
    /// Arquivos armazenados na sessão, caso o caminho não seja especificado
    /// </summary>
    private List<FormUploadFile> MemoryFiles
    {
        get => JJSession.GetSessionValue<List<FormUploadFile>>(MemoryFilesSessionName);
        set => JJSession.SetSessionValue(MemoryFilesSessionName, value);
    }

    /// <summary>
    /// Nome da variavél de sessão
    /// </summary>
    private string MemoryFilesSessionName => $"{Name}_jjfiles";

    /// <summary>
    /// Sempre aplica as alterações dos arquivos em disco, 
    /// se for falso mantem na memoria
    /// Default: true
    /// </summary>
    public bool AutoSave { get; set; }

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
    /// Caminho Completo do Diretório.<para></para>
    /// (Opcional) Se o caminho não for informado, todos os arquivos serão armazenado na sessão.
    /// </summary>
    /// <remarks>
    /// Exemplo: c:\temp\files\
    /// </remarks>
    public string FolderPath { get; set; }

    /// <summary>
    /// Objeto responsável por realizar upload dos arquivos
    /// </summary>
    public JJUploadFile Upload { get; set; }

    /// <summary>
    /// Componente GridView
    /// </summary>
    public JJGridView GridView
    {
        get
        {
            if (_gridView != null) return _gridView;

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
    /// Titulo do formulário.
    /// Default: Upload de Arquivos
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Sub-Titulo do formulário
    /// </summary>
    public string SubTitle { get; set; }

    /// <summary>
    /// Ação da grid responsável por realizar o upload do arquivo
    /// </summary>
    public ScriptAction DownloadAction
    {
        get
        {
            return _downloadAction ??= new ScriptAction
            {
                Icon = IconType.CloudDownload,
                ToolTip = "Download File",
                Name = "DOWNLOADFILE",
                OnClientClick = "jjview.downloadFile('" + Name + "','{NameJS}');"
            };
        }
        set => _downloadAction = value;
    }

    /// <summary>
    /// Ação da grid responsável por realizar a exclusão do arquivo
    /// </summary>
    public ScriptAction DeleteAction
    {
        get
        {
            if (_deleteAction != null) return _deleteAction;

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
        set => _deleteAction = value;
    }

    /// <summary>
    /// Ação da grid responsável por renomear um arquivo
    /// </summary>
    public ScriptAction RenameAction
    {
        get
        {
            if (_renameAction != null) return _renameAction;

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

    public bool ViewGallery { get; set; }

    public JJFormUpload()
    {
        Upload = new JJUploadFile();
        Title = "File Upload";
        ShowAddFile = true;
        Name = "jjuploadform1";
        CollapseAriaExpanded = true;
        AutoSave = true;
    }

    protected override string RenderHtml()
    {
        Upload.OnPostFile += UploadOnPostFile;

        var html = new StringBuilder();

        string previewImage = CurrentContext.Request["previewImage"];
        if (!string.IsNullOrEmpty(previewImage))
        {
            string fileName = Cript.Descript64(previewImage);
            var file = GetFile(fileName);
            
            if (file == null)
                return null;
            
            string src;
            if (file.IsInMemory)
            {
                src =  $"data:image/{Path.GetExtension(fileName).Replace(".","")};base64,{Convert.ToBase64String(file.FileStream.ToArray())}";
            }
            else
            {
                var filePath = Path.Combine(FolderPath,fileName);
                var appPath = HttpContext.Current!.Request.ApplicationPath;

                if (!appPath.EndsWith("/"))
                    appPath += "/";

                var culture = CultureInfo.CurrentCulture.Name + "/";
                src = $"{appPath}{culture}MasterData/Form/Download?filePath={Cript.Cript64(filePath)}".Trim();
            }
         
            html.Append("<center>");
            html.AppendFormat(
                "<img src=\"{0}\" alt=\"{1}\" title=\"{1}\" class=\"img-responsive\" style=\"max-height:350px;display:none;\" id=\"img\" />",
                src, fileName);
            html.Append("</center>");

            html.AppendLine("<script type=\"text/javascript\"> ");
            html.AppendLine(" ");
            html.AppendLine("	$(document).ready(function () { ");
            html.AppendLine("   $('#img').css('max-height',window.innerHeight);");
            html.AppendLine("   $('#img').show('slow');");
            html.AppendLine("	}); ");
            html.AppendLine("</script>");

            return html.ToString();
        }

        string previewVideo = CurrentContext.Request["previewVideo"];
        if (!string.IsNullOrEmpty(previewVideo))
        {
            string fileName = Cript.Descript64(previewVideo);
            var video = GetFile(fileName);

            string srcVideo = "data:video/mp4;base64," +
                              Convert.ToBase64String(video.FileStream.ToArray(), 0, video.FileStream.ToArray().Length);

            html.Append("<center>");
            html.AppendFormat(
                "<video autoplay width=\"100%\" height=\"100%\"controls src=\"{0}\" id=\"video\" class=\"img-responsive\" id=\"video\">",
                srcVideo);
            html.Append("</video>");
            html.Append("</center>");

            html.AppendLine("<script type=\"text/javascript\"> ");
            html.AppendLine(" ");
            html.AppendLine("	$(document).ready(function () { ");
            html.AppendLine(
                "  window.parent.$('#popup-modal').find('.close').click(function(){$('#video').trigger('pause')})");
            html.AppendLine("   $('#video').css('max-height',window.innerHeight);");
            html.AppendLine("	}); ");
            html.AppendLine("</script>");

            return html.ToString();
        }

        string uploadAction = CurrentContext.Request["uploadaction_" + Name];

        if (!string.IsNullOrEmpty(uploadAction))
            html.Append(GetResponseAction(uploadAction));

        if (!string.IsNullOrEmpty(Title))
            html.Append(GetHtmlTitle());

        html.Append(GetHtmlForm());

        html.Append(ViewGallery ? GetHtmlGalleryView() : GetHtmlGridView());

        html.Append(GetHtmlPreviewModal());

        return html.ToString();
    }

    private string GetResponseAction(string uploadAction)
    {
        string fileName = CurrentContext.Request.Form("filename_" + Name);
        try
        {
            switch (uploadAction)
            {
                case "DELFILE":
                {
                    if (OnDeleteFile != null)
                    {
                        var args = new FormDeleteFileEventArgs(fileName);
                        OnDeleteFile.Invoke(this, args);

                        if (!string.IsNullOrEmpty(args.ErrorMessage))
                            throw new Exception(args.ErrorMessage);
                    }

                    DeleteFile(fileName);
                    break;
                }
                case "DOWNLOADFILE":
                {
                    if (OnDownloadFile != null)
                    {
                        var args = new FormDownloadFileEventArgs(fileName, null);
                        OnDownloadFile.Invoke(this, args);

                        if (!string.IsNullOrEmpty(args.ErrorMessage))
                            throw new Exception(args.ErrorMessage);
                    }

                    DownloadFile(Path.Combine(FolderPath, fileName));
                    break;
                }
                case "RENAMEFILE":
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

                    RenameFile(currentName, newName);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            return new JJMessageBox(ex.Message, MessageIcon.Warning).GetHtml();
        }

        return string.Empty;
    }

    private string GetHtmlTitle()
    {
        var title = new JJTitle(Title, SubTitle);
        return title.GetHtml();
    }

    private string GetHtmlForm()
    {
        var html = new StringBuilder();

        html.AppendLine(
            $"<input type=\"hidden\" id=\"uploadaction_{Name}\" name=\"uploadaction_{Name}\" value=\"\" />");
        html.AppendLine($"<input type=\"hidden\" id=\"filename_{Name}\" name=\"filename_{Name}\" />");
        html.AppendLine(string.Empty);

        if (!ShowAddFile) return html.ToString();

        html.AppendLine(
            $"<div class=\"{BootstrapHelper.PanelGroup}\" id=\"divNovo\" runat=\"server\" enableviewstate=\"false\">");
        html.AppendLine($"\t<div class=\"{BootstrapHelper.GetPanel("default")}\">");
        html.Append(
            $"\t\t<div class=\"{BootstrapHelper.GetPanelHeading("default")}\" href=\"#collapse1\" {BootstrapHelper.DataToggle}=\"collapse\" data-target=\"#collapse1\" aria-expanded=\"");
        html.Append(CollapseAriaExpanded ? "true" : "false");
        html.AppendLine("\">");
        html.AppendLine($"\t\t\t<h4 class=\"{BootstrapHelper.PanelTitle}\">");
        html.Append("\t\t\t\t<a>");
        html.Append(Translate.Key("New File"));
        html.AppendLine("</a>");
        html.AppendLine("\t\t\t</h4>");
        html.AppendLine("\t\t</div>");
        html.Append("\t\t<div id=\"collapse1\" ");
        if (BootstrapHelper.Version == 3)
        {
            html.Append("\" class=\"panel-collapse collapse ");
            if (CollapseAriaExpanded)
                html.Append("in ");
        }
        else
        {
            html.Append($"\" class=\"panel-collapse in collapse {(CollapseAriaExpanded ? "show" : string.Empty)} ");
        }


        html.AppendLine("\">");
        html.AppendLine($"\t\t\t<div class=\"{BootstrapHelper.PanelBody}\">");
        html.AppendLine("");
        if (!Upload.AllowedTypes.Equals("*"))
        {
            html.Append("\t\t\t\t<label>");
            html.Append(Translate.Key("File Type:"));
            html.Append(" ");
            html.Append(Upload.AllowedTypes);
            html.Append(" </label>");
        }

        html.AppendLine("");

        if (!Upload.Multiple && CountFiles() > 0)
            Upload.AddLabel = Translate.Key("Update");

        html.AppendLine(Upload.GetHtml());

        html.AppendLine("\t\t\t</div>");
        html.AppendLine("\t\t</div>");
        html.AppendLine("\t</div>");
        html.AppendLine("</div>");
        return html.ToString();
    }

    private string GetHtmlGridView()
    {
        if (GridView.DataSource == null &&
            GridView.FormElement == null)
        {
            var dt = GetDataTableFiles();

            if (dt == null) return GridView.GetHtml();

            GridView.FormElement = new FormElement(dt);
            GridView.DataSource = dt;
            GridView.FormElement.Title = Title;
            GridView.FormElement.SubTitle = SubTitle;
            GridView.FormElement.Fields["NameJS"].VisibleExpression = "val:0";

            if (GridView.FormElement.Fields.Contains("LastWriteTime"))
                GridView.FormElement.Fields["LastWriteTime"].Label = "Last Modified";
        }

        return GridView.GetHtml();
    }

    private string GetHtmlGalleryView()
    {
        var files = GetFiles();
        var html = new StringBuilder();

        if (files.Count > 0)
        {

            foreach (var ac in GridView.GridActions)
            {
                ac.IsGroup = false;
            }

            html.Append("<div class=\"row\">");
            foreach (var file in files)
            {
                html.Append("<div class=\"col-sm-3\">");
                html.Append("<ul class=\"list-group list-group-flush\">");
                html.Append(GetPreviewImage(file.FileName));
                html.AppendFormat($"<li class=\"list-group-item\"><b>{Translate.Key("Name")}: </b>{{0}}</li>",
                    file.FileName);
                html.AppendFormat($"<li class=\"list-group-item\"><b>{Translate.Key("Size")}: </b>{{0}} Bytes</li>",
                    file.SizeBytes);
                html.AppendFormat($"<li class=\"list-group-item\"><b>{Translate.Key("Last Modified")}: </b>{{0}}</li>",
                    file.LastWriteTime);
                html.AppendFormat("<li class=\"list-group-item\"><table class=\"table-gallery\">{0}</table></li>",
                    GridView.GetHtmlAction(ConvertToHashtable(file)));
                html.Append("</ul>");
                html.AppendLine("</div>");
            }

            html.Append("</div>");
        }

        return html.ToString();
    }
    

    private string GetPreviewImage(string fileName)
    {
        var html = new StringBuilder();
        switch (Path.GetExtension(fileName))
        {
            case ".png":
            case ".jpg":
            case ".jpeg":
                var file = GetFile(fileName);
                var url = CurrentContext.Request.AbsoluteUri;
                
                string src;
                string filePath = Path.Combine(FolderPath,fileName);
                
                if (file.IsInMemory)
                {
                    src =  $"data:image/{Path.GetExtension(fileName).Replace(".","")};base64,{Convert.ToBase64String(file.FileStream.ToArray())}";
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

                html.Append("<li class=\"list-group-item\"  >");
                html.AppendFormat("<a href=\"javascript:popup.show('{1}','{0}',4)\" >", url, fileName);
                html.AppendFormat("<img loading=\"lazy\" src=\"{0}\" alt=\"{1}\" title=\"{1}\" style=\"height:180px\" class=\"img-responsive\" />",src, fileName);
                html.Append("</a>");
                html.Append("</li>");
                break;
            case ".mp4":
                string videoUrl = CurrentContext.Request.AbsoluteUri;

                if (videoUrl.Contains("?"))
                    videoUrl += "&";
                else
                    videoUrl += "?";

                videoUrl += "previewVideo=";
                videoUrl += Cript.Cript64(fileName);

                html.Append("<li class=\"list-group-item text-center\" >");
                html.AppendFormat("<a href=\"javascript:popup.show('{1}','{0}',4)\" >", videoUrl, fileName);
                html.Append("<div style=\"height:180px;\">");
                html.AppendFormat("<span class=\"fa fa-play-circle\" title=\"{0}\" style=\"color:red;padding-top:45px;font-size:100px;\"></span>", fileName);
                html.Append("</div>");
                html.Append("</a>");
                html.Append("</li>");

                break;
            case ".pdf":
                html.Append("<li class=\"list-group-item text-center\" style=\"background-color:#f5f5f5\">");
                html.Append("<div style=\"height:180px;\">");
                html.AppendFormat("<span class=\"fa fa-file-pdf-o\" title=\"{0}\" style=\"color:red;padding-top:45px;font-size:100px;\"></span>", fileName);
                html.Append("</div>");
                html.Append("</li>");
                break;
            case ".pptx":
                html.Append("<li class=\"list-group-item text-center\" style=\"background-color:#f5f5f5\">");
                html.Append("<div style=\"height:180px;\">");
                html.AppendFormat("<span class=\"fa fa-file-powerpoint-o\" title=\"{0}\" style=\"color:red;padding-top:45px;font-size:100px;\"></span>", fileName);
                html.Append("</div>");
                html.Append("</li>");
                break;
            case ".docx":
                html.Append("<li class=\"list-group-item text-center\" style=\"background-color:#f5f5f5\">");
                html.Append("<div style=\"height:180px;\">");
                html.AppendFormat("<span class=\"fa fa-file-word-o\" title=\"{0}\" style=\"color:blue;padding-top:45px;font-size:100px;\"></span>", fileName);
                html.Append("</div>");
                html.Append("</li>");
                break;
            case ".csv":
            case ".txt":
                html.Append("<li class=\"list-group-item text-center\" style=\"background-color:#f5f5f5\">");
                html.Append("<div style=\"height:180px;\">");
                html.AppendFormat("<span class=\"fa fa-file-text-o\" title=\"{0}\" style=\"color:black;padding-top:45px;font-size:100px;\"></span>", fileName);
                html.Append("</div>");
                html.Append("</li>");
                break;
            case ".xls":
                html.Append("<li class=\"list-group-item text-center\" style=\"background-color:#f5f5f5\">");
                html.Append("<div style=\"height:180px;\">");
                html.AppendFormat("<span class=\"fa fa-file-excel-o\" title=\"{0}\" style=\"color:green;padding-top:45px;font-size:100px;\"></span>", fileName);
                html.Append("</div>");
                html.Append("</li>");
                break;
            case ".rar":
            case ".zip":
                html.Append("<li class=\"list-group-item text-center\" style=\"background-color:#f5f5f5\">");
                html.Append("<div style=\"height:180px;\">");
                html.AppendFormat("<span class=\"fa fa-file-zip-o\" title=\"{0}\" style=\"color:#d2bb1c;padding-top:45px;font-size:100px;\"></span>", fileName);
                html.Append("</div>");
                break;
            default:
                html.Append("<li class=\"list-group-item text-center\" style=\"background-color:#f5f5f5\">");
                html.Append("<div style=\"height:180px;\">");
                html.AppendFormat("<span class=\"fa fa-file-o\" title=\"{0}\" style=\"color:gray;padding-top:45px;font-size:100px;\"></span>", fileName);
                html.Append("</div>");
                html.Append("</li>");
                break;
        }

        return html.ToString();
    }

    private Hashtable ConvertToHashtable(FormUploadFile file)
    {
        Hashtable hash = new();
        hash.Add(FileName, file.FileName);
        hash.Add(LastWriteTime, file.LastWriteTime);
        hash.Add(Size, file.SizeBytes);
        hash.Add(FileNameJs, file.FileName.Replace("'","\\'"));

        return hash;
    }

    private string GetHtmlPreviewModal()
    {

        int bootstrapVersion = BootstrapHelper.Version;
        
        StringBuilder html = new();
        html.AppendFormat("<div id=\"preview_modal_{0}\" ", Upload.Name);
        html.Append("class=\"modal fade\" ");
        html.Append("tabindex=\"-1\" ");
        html.Append("role=\"dialog\" ");
        html.AppendLine("style=\"display:none\">");
        html.AppendLine("\t<div class=\"modal-dialog modal-lg\" role=\"document\">");
        html.AppendLine("\t\t<div class=\"modal-content\">");
        html.AppendLine("\t\t\t<div class=\"modal-header\">");
        html.Append("\t\t\t\t<h4 class=\"modal-title\">");
        html.Append(Translate.Key("Would you like to save the image below?"));
        if (bootstrapVersion < 5)
            html.Append(GetCloseBtnHtml());
        html.AppendLine("</h4>");
        if (bootstrapVersion >= 5)
            html.Append(GetCloseBtnHtml());
        html.AppendLine("\t\t\t</div>");
        html.AppendLine("\t\t\t<div class=\"modal-body\">");
        html.AppendLine("\t\t\t\t<div class=\"row\">");
        html.AppendLine("\t\t\t\t\t<div class=\"col-sm-12\">");
        html.AppendFormat("\t\t\t\t\t\t<label for=\"preview_filename_{0}\">", Upload.Name);
        html.Append(Translate.Key("File name"));
        html.AppendLine("</label>");
        html.AppendLine("\t\t\t\t\t\t<div class=\"input-group\">");
        html.Append('\t', 7);
        html.AppendFormat("<input id=\"preview_filename_{0}\" ", Upload.Name);
        html.AppendLine("type=\"text\" class=\"form-control\" value=\"image\">");
        html.AppendLine("\t\t\t\t\t\t\t<span class=\"input-group-addon\">.png</span>");
        html.AppendLine("\t\t\t\t\t\t</div>");
        html.AppendLine("\t\t\t\t\t</div>");
        html.AppendLine("\t\t\t\t</div>");
        html.AppendLine("\t\t\t\t<div class=\"row\">");
        html.AppendLine("\t\t\t\t\t<div class=\"col-sm-12\">");
        html.AppendLine("\t\t\t\t\t\t<hr />");
        html.AppendLine("\t\t\t\t\t</div>");
        html.AppendLine("\t\t\t\t\t<div class=\"col-sm-12\">");
        html.Append('\t', 6);
        html.AppendFormat("<img id=\"pastedimage_{0}\" ", Upload.Name);
        html.AppendLine("class=\"img-responsive\" style=\"max-height:350px;\" alt=\"Preview Image\" />");
        html.AppendLine("\t\t\t\t\t</div>");
        html.AppendLine("\t\t\t\t</div>");
        html.AppendLine("\t\t\t</div>");
        html.AppendLine("\t\t\t<div class=\"modal-footer\">");
        html.Append('\t', 4);
        html.AppendFormat("<button id=\"btnDoUpload_{0}\" ", Upload.Name);
        html.Append("type=\"button\" ");
        html.Append("class=\"btn btn-primary\">");
        html.Append(Translate.Key("Save"));
        html.AppendLine("</button>");
        html.Append('\t', 4);
        html.Append($"<button type=\"button\" class=\"{BootstrapHelper.DefaultButton}\" {BootstrapHelper.DataDismiss}=\"modal\">");
        html.Append(Translate.Key("Cancel"));
        html.AppendLine("</button>");
        html.AppendLine("\t\t\t</div>");
        html.AppendLine("\t\t</div>");
        html.AppendLine("\t</div>");
        html.AppendLine("</div>");

        return html.ToString();
    }

    private string GetCloseBtnHtml()
    {
        StringBuilder html = new();

        html.Append($"\t\t\t\t<button type=\"button\" class=\"{BootstrapHelper.Close}\" {BootstrapHelper.DataDismiss}=\"modal\" ");
        html.AppendFormat("aria-label=\"{0}\">", Translate.Key("Close"));
        html.Append($"<span aria-hidden=\"true\">{BootstrapHelper.CloseButtonTimes}</span>");
        html.AppendLine("</button>");

        return html.ToString();
    }
    
    public List<FormUploadFile> GetFiles()
    {
        List<FormUploadFile> files = null;
        
        if (!AutoSave || string.IsNullOrEmpty(FolderPath))
            files = MemoryFiles;

        return files ?? GetPhysicalFiles();
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
        var files = GetFiles();
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
    
    public void CreateFile(string fileName, MemoryStream memoryStream)
    {
        if (fileName?.LastIndexOf("\\") > 0)
            fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);

        if (AutoSave & !string.IsNullOrEmpty(FolderPath))
        {
            SavePhysicalFile(FolderPath, fileName, memoryStream);
        }
        else
        {
            var files = GetFiles();
            var currentFile = files.Find(x => x.FileName.Equals(fileName));
            if (currentFile == null)
            {
                var file = new FormUploadFile
                {
                    FileName = fileName,
                    FileStream = memoryStream,
                    LastWriteTime = DateTime.Now,
                    SizeBytes = memoryStream.Length
                };
                files.Add(file);
            }
            else
            {
                currentFile.Deleted = false;
                currentFile.FileStream = memoryStream;
                currentFile.LastWriteTime = DateTime.Now;
                currentFile.SizeBytes = memoryStream.Length;
            }

            MemoryFiles = files;

        }
    }
    
    public void DeleteFile(string fileName)
    {
        if (AutoSave & !string.IsNullOrEmpty(FolderPath))
        {
            File.Delete(FolderPath + fileName);
        }
        else
        {
            var files = GetFiles();
            var file = files.Find(x => x.FileName.Equals(fileName));
            if (file != null)
            {
                if (!file.IsInMemory)
                    file.Deleted = true;
                else
                    files.Remove(file);
            }

            MemoryFiles = files;
        }
    }
    
    public void DownloadFile(string fileName)
    {
        var download = new JJDownloadFile(fileName);

        download.ResponseDirectDownload();
    }
    
    public void RenameFile(string currentName, string newName)
    {
        if (string.IsNullOrEmpty(currentName))
            throw new ArgumentException(nameof(currentName));

        if (string.IsNullOrWhiteSpace(newName))
            throw new Exception(Translate.Key("Required file name"));

        if (!Validate.ValidFileName(newName))
            throw new Exception(Translate.Key("file name cannot contain [{0}] characters", "* < > | : ? \" / \\"));

        if (!FileIO.GetFileNameExtension(currentName).Equals(FileIO.GetFileNameExtension(newName)))
            throw new Exception(Translate.Key("The file extension must remain the same"));

        var files = GetFiles();
        if (files.Exists(x => x.FileName.Equals(newName)))
            throw new Exception(Translate.Key("A file with the name {0} already exists", newName));

        if (AutoSave & !string.IsNullOrEmpty(FolderPath))
        {
            File.Move(FolderPath + currentName, FolderPath + newName);
        }
        else
        {
            var file = files.Find(x => x.FileName.Equals(currentName));
            if (file == null)
                throw new Exception(Translate.Key("file {0} not found!", currentName));

            file.FileName = newName;
            if (file.FileStream == null & string.IsNullOrEmpty(file.OriginName))
                file.OriginName = currentName;

            MemoryFiles = files;
        }
    }

    /// <summary>
    /// Recupera o conteúdo do arquivo
    /// </summary>
    /// <param name="fileName">Nome do arquivo</param>
    /// <returns>
    /// Stream com o conteúdo do arquivo
    /// </returns>
    public FormUploadFile GetFile(string fileName)
    {
        var files = GetFiles();
        return files.Find(x => x.FileName.Equals(fileName));
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

    /// <summary>
    /// Save the files from the memory to the disk.
    /// </summary>
    public void SaveMemoryFiles(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            throw new ArgumentNullException(nameof(folderPath));
        }

        if (MemoryFiles == null)
            return;

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        foreach (var file in MemoryFiles)
        {
            if (file.Deleted)
            {
                string filename = string.IsNullOrEmpty(file.OriginName) ? file.FileName : file.OriginName;
                File.Delete(folderPath + filename);
            }
            else if (!string.IsNullOrEmpty(file.OriginName))
            {
                File.Move(folderPath + file.OriginName, folderPath + file.FileName);
            }
            else if (file.FileStream != null && file.IsInMemory)
            {
                SavePhysicalFile(folderPath, file.FileName, file.FileStream);
            }
        }

        FolderPath = folderPath;
        MemoryFiles = null;
    }
    
    public void ClearMemoryFiles()
    {
        MemoryFiles = null;
    }
    
    public void DeleteAll()
    {
        if (!string.IsNullOrEmpty(FolderPath))
        {
            if (Directory.Exists(FolderPath))
                Directory.Delete(FolderPath, true);
        }

        MemoryFiles = null;
    }
    
    public int CountFiles()
    {
        var listFiles = GetFiles();
        return listFiles.Count(x => !x.Deleted);
    }

    private void SavePhysicalFile(string folderPath, string fileName, MemoryStream ms)
    {
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fileFullName = folderPath + fileName;

        var fileStream = File.Create(fileFullName);
        ms.Seek(0, SeekOrigin.Begin);
        ms.CopyTo(fileStream);
        fileStream.Close();
    }

    /// <summary>
    /// Recupera os arquivos salvos na pasta
    /// </summary>
    private List<FormUploadFile> GetPhysicalFiles()
    {
        var formfiles = new List<FormUploadFile>();
        if (string.IsNullOrEmpty(FolderPath))
            return formfiles;

        var oDir = new DirectoryInfo(FolderPath);
        if (oDir.Exists)
        {
            FileInfo[] files = oDir.GetFiles();
            foreach (FileInfo oFile in files)
            {
                var formfile = new FormUploadFile();
                formfile.FileName = oFile.Name;
                formfile.SizeBytes = oFile.Length;
                formfile.LastWriteTime = oFile.LastWriteTime;
                formfiles.Add(formfile);
            }
        }
        return formfiles;
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
            if (!Upload.Multiple && CountFiles() > 0)
            {
                foreach (var file in GetFiles())
                {
                    DeleteFile(file.FileName);
                }
            }

#if NETFRAMEWORK
            var stream = new MemoryStream();
            e.File.FileData.InputStream.CopyTo(stream);
            CreateFile(e.File.FileData.FileName, stream);
#else
            using var stream = new MemoryStream();
            e.File.FileData.CopyTo(stream);
            CreateFile(e.File.FileData.FileName, stream);
#endif


        }
        catch (Exception ex)
        {
            e.ErrorMessage = ex.Message;
        }

    }

}
