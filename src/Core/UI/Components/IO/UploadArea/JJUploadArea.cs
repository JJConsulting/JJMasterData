using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
    public string AllowedTypes { get; set; } = "*";

    /// <summary>
    /// Allows simultaneous upload of files.
    /// Default: True
    /// </summary>
    public bool Multiple { get; set; } = true;

    /// <remarks>
    /// Default = "Add"
    /// </remarks>
    public string AddLabel { get; set; } = "Add";

    /// <remarks>
    /// Default = "Cancel"
    /// </remarks>
    public string CancelLabel { get; set; } = "Cancel";

    /// <remarks>
    /// Default = "Stop"
    /// </remarks>
    public string AbortLabel { get; set; } = "Stop";

    /// <remarks>
    /// Default = "is not allowed. Allowed extensions: "
    /// </remarks>
    public string ExtensionNotAllowedLabel { get; set; } = "is not allowed. Allowed extensions: ";

    /// <remarks>
    /// Default = "is not allowed. Allowed Max size: "
    /// </remarks>
    public string SizeErrorLabel { get; set; } = "is not allowed. Allowed Max size: ";

    /// <remarks>
    /// Default = "Paste or Drag and Drop Files"
    /// </remarks>
    public string DragDropLabel{ get; set; } = "Paste or Drag & Drop Files";

    public bool EnableDragDrop { get; set; } = true;

    public bool EnableCopyPaste { get; set; } = true;

    /// <remarks>
    /// Default = True 
    /// </remarks>
    public bool ShowFileSize { get; set; } = true;

    /// <remarks>
    /// Measured in bytes
    /// </remarks>
    public int MaxFileSize { get; set; }
    
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
    
    private string AreFilesUploadedFieldName =>  $"{Name}-are-files-uploaded";

    public string JsCallback { get; set; } = @"document.forms[0].submit()";

    public Dictionary<string, string> QueryStringParams { get; } = new();
    
    public int ParallelUploads { get; set; } = 1;
    
    public JJUploadArea(
        IHttpContext currentContext,
        IUploadAreaService uploadAreaService,
        JJMasterDataUrlHelper urlHelper,
        IEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        Name = "upload-area";
        CurrentContext = currentContext;
        UploadAreaService = uploadAreaService;
        UrlHelper = urlHelper;
        StringLocalizer = stringLocalizer;
        EncryptionService = encryptionService;
        MaxFileSize = GetMaxRequestLength();
    }
    
    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (IsPostAfterUploadAllFiles())
            return await GetFileUploadResultAsync();
        
        return new RenderedComponentResult(GetUploadAreaHtmlBuilder());
    }

    public async Task<ComponentResult> GetFileUploadResultAsync()
    {
        if (OnFileUploaded != null)
            UploadAreaService.OnFileUploaded += OnFileUploaded;

        if (OnFileUploadedAsync != null)
            UploadAreaService.OnFileUploadedAsync += OnFileUploadedAsync;

        var result = await UploadAreaService.UploadFileAsync("uploadAreaFile[0]", AllowedTypes);
        return new JsonComponentResult(result);
    }

    internal HtmlBuilder GetUploadAreaHtmlBuilder()
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", $"{Name}-upload-area-div")
            .WithCssClass("upload-area-div")
            .AppendHiddenInput(AreFilesUploadedFieldName)
            .Append(HtmlTag.Div,  div =>
            {
                div.WithAttributes(Attributes);
                div.WithCssClass("dropzone");
                div.WithAttribute("id", Name);
            });
        div.WithAttributes(Attributes);
        div.WithAttribute("js-callback",JsCallback);
        div.WithAttribute("route-context", EncryptionService.EncryptRouteContext(RouteContext));
        div.WithAttribute("allow-multiple-files", Multiple.ToString().ToLower());
        div.WithAttribute("query-string-params", GetQueryStringParams());
        div.WithAttribute("max-file-size", MaxFileSize.ToString().ToLower());
        div.WithAttribute("allow-drag-drop", EnableDragDrop.ToString().ToLower());
        div.WithAttribute("allow-copy-paste", EnableCopyPaste.ToString().ToLower());
        div.WithAttribute("show-file-size", ShowFileSize.ToString().ToLower());
        div.WithAttribute("allowed-types", AllowedTypes);
        div.WithAttribute("parallel-uploads", ParallelUploads);
        div.WithAttribute("add-file-label", StringLocalizer[AddLabel]);
        div.WithAttribute("drag-drop-label", StringLocalizer[DragDropLabel]);
        div.WithAttribute("cancel-label", StringLocalizer[CancelLabel]);
        div.WithAttribute("abort-label", StringLocalizer[AbortLabel]);
        div.WithAttribute("extension-not-allowed-label", StringLocalizer[ExtensionNotAllowedLabel]);
        div.WithAttribute("file-size-error-label", StringLocalizer[SizeErrorLabel]);
        
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
        string action = CurrentContext.Request.GetFormValue(AreFilesUploadedFieldName);
        return "1".Equals(action);
    }
    
    public string GetQueryStringParams()
    {
        if (QueryStringParams.Count == 0)
            return string.Empty;

        var keyValuePairs = 
            from kvp in QueryStringParams
            let key = HttpUtility.UrlEncode(kvp.Key) 
            let value = HttpUtility.UrlEncode(kvp.Value)
            select $"{key}={value}";
        return string.Join("&", keyValuePairs);
    }
    
}
