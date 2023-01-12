using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using System;
using System.Collections.Generic;
using System.IO;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.Http.Abstractions;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

public class JJUploadArea : JJBaseView
{
    /// <summary>
    /// Event fired when rendering HTML content
    /// </summary>  
    /// <remarks>
    /// To perform validations, just return the error message in the function. 
    /// If the function returns something other than null, the content will be displayed as an error.
    /// </remarks>
    public event EventHandler<FormUploadFileEventArgs> OnPostFile;

    /// <summary>
    /// Allowed extension type, separated by comma.
    /// Default: *
    /// Example: txt,csv,log
    /// </summary>
    /// <remarks>
    /// On the systems file types are blocked like .exe .dll etc...
    /// </remarks>
    public string AllowedTypes { get; set; }

    /// <summary>
    /// Allows simultaneous upload of files.
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
    
    internal IHttpContext HttpContext { get; }
    
    public JJUploadArea(IHttpContext httpContext)
    {
        AllowedTypes = "*";
        Name = "uploadFile1";
        Multiple = true;
        HttpContext = httpContext;
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
    
    internal override HtmlBuilder RenderHtml()
    {
        string requestType = HttpContext.Request.QueryString("t");
        if ("jjupload".Equals(requestType))
        {
            UploadFile();
            return null;
        }

        return GetFieldHtmlElement();
    }

    private HtmlBuilder GetFieldHtmlElement()
    {
        var div = new HtmlBuilder(HtmlTag.Div)
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
        if (System.Configuration.ConfigurationManager.GetSection("system.web/httpRuntime") is System.Web.Configuration.HttpRuntimeSection section)
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
    private FormFileContent GetFile()
    {
        var fileData = HttpContext.Request.GetFile("file");
        using var stream = new MemoryStream();
        string filename = fileData.FileName;
        
#if NETFRAMEWORK
        fileData.InputStream.CopyTo(stream);
#else
        fileData.CopyTo(stream);
#endif

        var content = new FormFileContent
        {
            FileName = filename,
            Bytes = stream.ToArray(),
            Length = stream.Length,
            LastWriteTime = DateTime.Now
        };

        return content;
    }
    private record UploadAreaDto
    {
        [JsonProperty("jquery-upload-file-message", NullValueHandling=NullValueHandling.Ignore)]
        public string Message { get; set; }
        
        [JsonProperty("jquery-upload-file-error", NullValueHandling=NullValueHandling.Ignore)]
        public string Error { get; set; }
        public string ToJson() => JsonConvert.SerializeObject(this);
    }
    private void UploadFile()
    {
        UploadAreaDto dto = new();
        
        try
        {
            string message = string.Empty;
            
            var file = GetFile();
            
            ValidateSystemFiles(file.FileName);

            if (OnPostFile != null)
            {
                var args = new FormUploadFileEventArgs(file);
                OnPostFile.Invoke(this, args);
                var errorMessage = args.ErrorMessage;
                if (args.SuccessMessage != null)
                {
                    message = args.SuccessMessage;
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    throw new JJMasterDataException(errorMessage);
            }

            dto.Message = message;

        }
        catch (Exception ex)
        {
            dto.Error = ex.Message;
        }

        HttpContext.Response.SendResponse(dto.ToJson(),"text/json");
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
            throw new JJMasterDataException(Translate.Key("You cannot upload system files"));

    }
    public bool IsPostAfterUploadAllFiles()
    {
        string nameField = $"uploadaction_{Name}";
        string action = HttpContext.Request[nameField];
        return "afteruploadall".Equals(action);
    }

}
