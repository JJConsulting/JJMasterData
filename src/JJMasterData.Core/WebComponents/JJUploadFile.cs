using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
#if NETFRAMEWORK
using System.Web.Configuration;
#endif
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.FormEvents.Args;


namespace JJMasterData.Core.WebComponents;

public class JJUploadFile : JJBaseView
{
    public delegate string OnPostFileAction(JJFormFile file);

    /// <summary>
    /// Evento disparado ao renderizar o conteúdo HTML.
    /// </summary>  
    /// <remarks>
    /// Para realizar validações, basta retornar a mensagem de erro na função. 
    /// Se a função retornar diferente de nulo o conteúdo será exibido como erro.
    /// </remarks>
    public event EventHandler<FormUploadFileEventArgs> OnPostFile;

    /// <summary>
    /// Tipod de extensão permitida, separados por virgula.
    /// Default: *
    /// Example: txt,csv,log
    /// </summary>
    /// <remarks>
    /// Em * os tipos de arquivos de sistemas são bloqueados como .exe .dll etc...
    /// </remarks>
    public string AllowedTypes { get; set; }

    /// <summary>
    /// Permite upload simultaneo de arquivos.
    /// Default: True
    /// </summary>
    public bool Multiple { get; set; }

    /// <summary>
    /// String botão Upload 
    /// Default = "Add"
    /// </summary>
    public string LabelAdd { get; set; }

    /// <summary>
    /// String Done
    /// Default = "Done"
    /// </summary>
    public string LabelDone { get; set; }

    /// <summary>
    /// String Cancel
    /// Default = "Cancel"
    /// </summary>
    public string LabelCancel { get; set; }

    /// <summary>
    /// String Stop
    /// Default = "Stop"
    /// </summary>
    public string LabelStop { get; set; }

    /// <summary>
    /// String Extension Error
    /// Default = "is not allowed. Allowed extensions: "
    /// </summary>
    public string LabelExtError { get; set; }

    /// <summary>
    /// String Size Error
    /// Default = "is not allowed. Allowed Max size: "
    /// </summary>
    public string LabelSizeError { get; set; }

    /// <summary>
    /// String Drag and Drop
    /// Default = "Paste or Drag and Drop Files"
    /// </summary>
    public string LabelDragDrop{ get; set; }

    /// <summary>
    /// Habilita Drag and Drop
    /// Default = True
    /// </summary>
    public bool DragDrop { get; set; }

    /// <summary>
    /// Habilita Colar print de tela
    /// Default = True
    /// </summary>
    public bool CopyPaste { get; set; }

    /// <summary>
    /// Exibir o tamanho do arquivo
    /// Default = True 
    /// </summary>
    public bool ShowFileSize { get; set; }

    /// <summary>
    /// Tamanho máximo do arquivo em bytes
    /// </summary>
    public int MaxFileSize { get; set; }

    /// <summary>
    /// Executa um postback na pagina após o upload de todos os arquivos
    /// Default = True 
    /// </summary>
    public bool AutoSubmitAfterUploadAll { get; set; }


    public JJUploadFile()
    {
        AllowedTypes = "*";
        Name = "uploadFile1";
        Multiple = true;
        DragDrop = true;
        CopyPaste = true;
        ShowFileSize = true;
        AutoSubmitAfterUploadAll = true;
        LabelAdd = "Add";
        LabelDone = "Done";
        LabelCancel = "Cancel";
        LabelStop = "Stop";
        LabelExtError = "is not allowed. Allowed extensions: ";
        LabelSizeError = "is not allowed. Allowed Max size: ";
        LabelDragDrop = "Paste or Drag & Drop Files";

        MaxFileSize = GetMaxRequestLength();
    }

    protected override string RenderHtml()
    {
        //Se o post for via ajax
        string t = CurrentContext.Request.QueryString("t");
        if ("jjupload".Equals(t))
        {
            UploadFile();
            return null;
        }

        return GetHtmlField();
    }

    private string GetHtmlField()
    {
        StringBuilder html = new();

        string nameaction = string.Format("uploadaction_{0}", Name);
        html.AppendLine("\t<div id=\"divupload\">");
        html.AppendFormat("\t\t<input type=\"hidden\" id=\"{0}\" name=\"{0}\" value=\"\" />\r\n", nameaction);
        html.AppendLine("\t\t<div class=\"areaArquivos\">");
        html.Append("\t\t\t<div ");
        html.Append("class=\"fileUpload\" ");
        html.AppendFormat("id =\"{0}\" ", Name);
        html.AppendFormat("jjmultiple=\"{0}\" ", Multiple ? "true" : "false");
        html.AppendFormat("maxFileSize=\"{0}\" ", MaxFileSize);
        html.AppendFormat("dragDrop=\"{0}\" ", DragDrop ? "true" : "false");
        html.AppendFormat("copyPaste=\"{0}\" ", CopyPaste ? "true" : "false");
        html.AppendFormat("showFileSize=\"{0}\" ", ShowFileSize ? "true" : "false");
        html.AppendFormat("autoSubmit=\"{0}\" ", AutoSubmitAfterUploadAll ? "true" : "false");
        html.AppendFormat("allowedTypes=\"{0}\" ", AllowedTypes);
        html.AppendFormat("uploadStr=\"{0}\" ", Translate.Key(LabelAdd));
        html.AppendFormat("dragDropStr=\"{0}\" ", Translate.Key(LabelDragDrop));
        html.AppendFormat("doneStr=\"{0}\" ", Translate.Key(LabelDone));
        html.AppendFormat("cancelStr=\"{0}\" ", Translate.Key(LabelCancel));
        html.AppendFormat("abortStr=\"{0}\" ", Translate.Key(LabelStop));
        html.AppendFormat("extErrorStr=\"{0}\" ", Translate.Key(LabelExtError));
        html.AppendFormat("sizeErrorStr=\"{0}\" ", Translate.Key(LabelSizeError));
        html.AppendLine("></div>");
        html.AppendLine("\t\t</div>");
        html.AppendLine("\t</div>");

        return html.ToString();
    }

    /// <summary>
    /// Recupera o tamanho máximo permitido para upload
    /// </summary>
    /// <remarks>
    /// Para alterar o tamanho máximo permito
    /// Configure a chave httpRuntime maxRequestLength...
    /// no Web.Config 
    /// </remarks>
    /// <returns>Tamanho em bytes</returns>
    public int GetMaxRequestLength()
    {
        int maxRequestLength;

#if NETFRAMEWORK
        maxRequestLength = 4194304; //4mb
        if (ConfigurationManager.GetSection("system.web/httpRuntime") is HttpRuntimeSection section)
            maxRequestLength = section.MaxRequestLength * 1024;
#else

        // ASP.NET Core enforces 30MB (~28.6 MiB) max request body size limit, be it Kestrel and HttpSys.
        // Under normal circumstances, there is no need to increase the size of the HTTP request.

        maxRequestLength = 30720000;
#endif

        return maxRequestLength;
    }

    /// <summary>
    /// Recupera o arquivo após o post
    /// </summary>
    private JJFormFile GetFile() => new(CurrentContext.Request.GetFile("file"));
    private void UploadFile()
    {
        string data;
        try
        {
            string messageInfo = string.Empty;
            
            var file = GetFile();
            
            ValidateSystemFiles(file.FileData.FileName);

            if (OnPostFile != null)
            {
                var args = new FormUploadFileEventArgs(file);
                OnPostFile.Invoke(this, args);
                var errorMessage = args.ErrorMessage;
                if (args.SuccessMessage != null)
                {
                    messageInfo = args.SuccessMessage;
                    messageInfo = messageInfo.Replace("\\", "\\\\");
                    messageInfo = messageInfo.Replace("\"", "'");
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    throw new Exception(errorMessage);
            }

            data = "{\"jquery-upload-file-message\": \"" + messageInfo + "\"}";

        }
        catch (Exception ex)
        {
            string errMsg = ex.Message;
            errMsg = errMsg.Replace("\\", "\\\\");
            errMsg = errMsg.Replace("\"", "'");
            data="{\"jquery-upload-file-error\": \"" + errMsg + "\"}";
        }

        CurrentContext.Response.SendResponse(data,"text/json");
    }

    private void ValidateSystemFiles(string filename)
    {
        if (!AllowedTypes.Equals("*"))
            return;

        var list = new List<string>
        {
            ".ade",
            ".adp",
            ".apk",
            ".appx",
            ".appxbundle",
            ".bat",
            ".cab",
            ".chm",
            ".cmd",
            ".com",
            ".cpl",
            ".dll",
            ".dmg",
            ".ex",
            ".ex_",
            ".exe",
            ".hta",
            ".ins",
            ".isp",
            ".iso",
            ".js",
            ".jse",
            ".lib",
            ".lnk",
            ".mde",
            ".msc",
            ".msi",
            ".msix",
            ".msixbundle",
            ".msp",
            ".mst",
            ".nsh",
            ".pif",
            ".ps1",
            ".scr",
            ".sct",
            ".shb",
            ".sys",
            ".vb",
            ".vbe",
            ".vbs",
            ".vxd",
            ".wsc",
            ".wsf",
            ".wsh",
            ".jar",
            ".cs",
            ".bin"
        };

        string ext = FileIO.GetFileNameExtension(filename);
        if (list.Contains(ext))
            throw new Exception(Translate.Key("You cannot upload system files"));

    }

    /// <summary>
    /// Se for post depois de subir todos os arquivos
    /// </summary>
    public bool IsPostAfterUploadAll()
    {
        string namefield = string.Format("uploadaction_{0}", Name);
        string action = CurrentContext.Request[namefield];
        return "afteruploadall".Equals(action);
    }

}
