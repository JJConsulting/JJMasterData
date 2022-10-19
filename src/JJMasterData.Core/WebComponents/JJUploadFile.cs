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
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

/// TODO: Breaking change suggestion: JJUploadField, JJUploadFile sounds like a action in English.
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

    /// <remarks>
    /// Default = "Add"
    /// </remarks>
    public string AddLabel { get; set; }

    /// <remarks>
    /// Default = "Done"
    /// </remarks>
    public string DoneLabel { get; set; }

    /// <remarks>
    /// Default = "Cancel"
    /// </remarks>
    public string CancelLabel { get; set; }

    /// <remarks>
    /// Default = "Stop"
    /// </remarks>
    public string AbortLabel { get; set; }

    /// <remarks>
    /// Default = "is not allowed. Allowed extensions: "
    /// </remarks>
    public string NotAllowedExtensionErrorLabel { get; set; }

    /// <remarks>
    /// Default = "is not allowed. Allowed Max size: "
    /// </remarks>
    public string SizeErrorLabel { get; set; }

    /// <remarks>
    /// Default = "Paste or Drag and Drop Files"
    /// </remarks>
    public string DragDropLabel{ get; set; }
    
    public bool EnableDragDrop { get; set; }
    
    public bool EnableCopyPaste { get; set; }

    /// <remarks>
    /// Default = True 
    /// </remarks>
    public bool ShowFileSize { get; set; }

    /// <remarks>
    /// Measured in bytes
    /// </remarks>
    public int MaxFileSize { get; set; }
    
    public bool AutoSubmitAfterUploadAll { get; set; }
    
    public JJUploadFile()
    {
        AllowedTypes = "*";
        Name = "uploadFile1";
        Multiple = true;
        EnableDragDrop = true;
        EnableCopyPaste = true;
        ShowFileSize = true;
        AutoSubmitAfterUploadAll = true;
        AddLabel = "Add";
        DoneLabel = "Done";
        CancelLabel = "Cancel";
        AbortLabel = "Stop";
        NotAllowedExtensionErrorLabel = "is not allowed. Allowed extensions: ";
        SizeErrorLabel = "is not allowed. Allowed Max size: ";
        DragDropLabel = "Paste or Drag & Drop Files";

        MaxFileSize = GetMaxRequestLength();
    }
    
    internal override HtmlElement RenderHtmlElement()
    {
        string requestType = CurrentContext.Request.QueryString("t");
        if ("jjupload".Equals(requestType))
        {
            UploadFile();
            return null;
        }

        return GetFieldHtmlElement();
    }

    private HtmlElement GetFieldHtmlElement()
    {
        var div = new HtmlElement(HtmlTag.Div)
            .WithAttribute("id", "divupload")
            .AppendHiddenInput($"uploadaction_{Name}", string.Empty)
            .AppendElement(HtmlTag.Div,  div =>
                {
                    div.WithCssClass("fileUpload");
                    div.WithAttribute("id", Name);
                    div.WithAttribute("jjmultiple", Multiple.ToString().ToLower());
                    div.WithAttribute("maxFileSize", MaxFileSize.ToString().ToLower());
                    div.WithAttribute("dragDrop", EnableDragDrop.ToString().ToLower());
                    div.WithAttribute("copyPaste", EnableCopyPaste.ToString().ToLower());
                    div.WithAttribute("showFileSize", ShowFileSize.ToString().ToLower());
                    div.WithAttribute("autoSubmit", AutoSubmitAfterUploadAll.ToString().ToLower());
                    div.WithAttribute("allowedTypes", AllowedTypes);
                    div.WithAttribute("uploadStr", Translate.Key(AddLabel));
                    div.WithAttribute("dragDropStr", Translate.Key(DragDropLabel));
                    div.WithAttribute("doneStr", Translate.Key(DoneLabel));
                    div.WithAttribute("cancelStr", Translate.Key(CancelLabel));
                    div.WithAttribute("abortStr", Translate.Key(AbortLabel));
                    div.WithAttribute("extErrorStr", Translate.Key(NotAllowedExtensionErrorLabel));
                    div.WithAttribute("sizeErrorStr", Translate.Key(SizeErrorLabel));
                });

        return div;
    }

    /// <remarks>
    /// To change this in .NET Framework, change web.config in system.web/httpRuntime
    /// Measured in bytes
    /// </remarks>
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
    /// Recovers the file after the POST
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
    public bool IsPostAfterUploadAllFiles()
    {
        string namefield = string.Format("uploadaction_{0}", Name);
        string action = CurrentContext.Request[namefield];
        return "afteruploadall".Equals(action);
    }

}
