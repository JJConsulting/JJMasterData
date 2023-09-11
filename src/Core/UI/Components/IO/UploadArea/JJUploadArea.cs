using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;
using System.IO;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Components;

public class JJUploadArea : AsyncComponent
{
    /// <summary>
    /// Event fired when the file is posted.
    /// </summary>  
    public event EventHandler<FormUploadFileEventArgs> OnFileUploaded;
    public event AsyncEventHandler<FormUploadFileEventArgs> OnFileUploadedAsync;
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
    public string ExtensionNotAllowedLabel { get; set; }

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

    internal IHttpContext CurrentContext { get; }
    private IUploadAreaService UploadAreaService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    internal IEncryptionService EncryptionService { get; }
    
    private RouteContext _routeContext;
    internal RouteContext RouteContext
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
    
    internal ComponentContext ComponentContext => RouteContext.ComponentContext;
    
    private string IsFilesUploadedFieldName =>  $"{Name}-is-files-uploaded";

    public string JsCallback { get; set; } = @"document.forms[0].submit()";

    public JJUploadArea(
        IHttpContext currentContext,
        IUploadAreaService uploadAreaService,
        JJMasterDataUrlHelper urlHelper,
        IEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        CurrentContext = currentContext;
        UploadAreaService = uploadAreaService;
        UrlHelper = urlHelper;
        StringLocalizer = stringLocalizer;
        AllowedTypes = "*";
        Name = "uploadFile1";
        Multiple = true;
        EnableDragDrop = true;
        EnableCopyPaste = true;
        EncryptionService = encryptionService;
        ShowFileSize = true;
        AutoSubmitAfterUploadAll = true;
        AddLabel = "Add";
        DoneLabel = "Done";
        CancelLabel = "Cancel";
        AbortLabel = "Stop";
        ExtensionNotAllowedLabel = "is not allowed. Allowed extensions: ";
        SizeErrorLabel = "is not allowed. Allowed Max size: ";
        DragDropLabel = "Paste or Drag & Drop Files";

        MaxFileSize = GetMaxRequestLength();
    }
    
    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (ComponentContext is ComponentContext.FileUpload)
        {
            return await GetFileUploadResultAsync();
        }

        return new RenderedComponentResult(GetUploadAreaHtmlBuilder());
    }

    public async Task<ComponentResult> GetFileUploadResultAsync()
    {
        if (OnFileUploaded != null)
            UploadAreaService.OnFileUploaded += OnFileUploaded;

        if (OnFileUploadedAsync != null)
            UploadAreaService.OnFileUploadedAsync += OnFileUploadedAsync;

        var result = await UploadAreaService.UploadFileAsync("uploadAreaFile", AllowedTypes);
        return new JsonComponentResult(result);
    }

    internal HtmlBuilder GetUploadAreaHtmlBuilder()
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", "upload-area-div")
            .AppendHiddenInput(IsFilesUploadedFieldName)
            .Append(HtmlTag.Div,  div =>
                {
                    div.WithCssClass("fileUpload");
                    div.WithAttributes(Attributes);
                    div.WithAttribute("id", Name);
                    div.WithAttribute("js-callback",JsCallback);
                    div.WithAttribute("routecontext", EncryptionService.EncryptRouteContext(RouteContext));
                    div.WithAttribute("jjmultiple", Multiple.ToString().ToLower());
                    div.WithAttribute("maxFileSize", MaxFileSize.ToString().ToLower());
                    div.WithAttribute("dragDrop", EnableDragDrop.ToString().ToLower());
                    div.WithAttribute("copyPaste", EnableCopyPaste.ToString().ToLower());
                    div.WithAttribute("showFileSize", ShowFileSize.ToString().ToLower());
                    div.WithAttribute("autoSubmit", AutoSubmitAfterUploadAll.ToString().ToLower());
                    div.WithAttribute("allowedTypes", AllowedTypes);
                    div.WithAttribute("uploadStr", StringLocalizer[AddLabel]);
                    div.WithAttribute("dragDropStr", StringLocalizer[DragDropLabel]);
                    div.WithAttribute("doneStr", StringLocalizer[DoneLabel]);
                    div.WithAttribute("cancelStr", StringLocalizer[CancelLabel]);
                    div.WithAttribute("abortStr", StringLocalizer[AbortLabel]);
                    div.WithAttribute("extErrorStr", StringLocalizer[ExtensionNotAllowedLabel]);
                    div.WithAttribute("sizeErrorStr", StringLocalizer[SizeErrorLabel]);
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
    
    public bool IsPostAfterUploadAllFiles()
    {
        string action = CurrentContext.Request.GetFormValue(IsFilesUploadedFieldName);
        return "1".Equals(action);
    }

}
