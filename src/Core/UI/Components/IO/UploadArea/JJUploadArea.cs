#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Html;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class JJUploadArea : AsyncComponent
{
    /// <summary>
    /// Event fired when the file is posted.
    /// </summary>  
    public event EventHandler<FormUploadFileEventArgs>? OnFileUploaded;
    
    /// <summary>
    /// Async event fired when the file is posted.
    /// </summary>  
    public event AsyncEventHandler<FormUploadFileEventArgs>? OnFileUploadedAsync;
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
    
    
    public string CancelLabel { get; set; } = "Cancel";
    
    public string AbortLabel { get; set; } = "Stop";
    
    public string ExtensionNotAllowedLabel { get; set; } = "File type is not allowed. Allowed extensions: {0}";
    
    public string SizeErrorLabel { get; set; } = "File is too big. Max file size: {0} mb";

    public string GetImportLabel()
    {
        if (EnableDragDrop && !EnableCopyPaste)
            return "Click here to upload or drag & drop files";

        if (!EnableDragDrop && !EnableCopyPaste)
            return "Click here to upload files";
        
        if (!EnableDragDrop && EnableCopyPaste)
            return "Click here to upload or paste files";

        return "Click here, paste or drag & drop files";
    }

    public bool EnableDragDrop { get; set; } = true;

    public bool EnableCopyPaste { get; set; } = true;
    public bool ShowFileSize { get; set; } = true;
    public int MaxFileSize { get; set; }

    /// <summary>
    /// JS code to be executed after all server side uploads are completed.
    /// </summary>
    public string JsCallback { get; set; } = @"document.forms[0].submit()";

    /// <summary>
    /// QueryString parameters to be sended at async POST requests.
    /// </summary>
    public Dictionary<string, string> QueryStringParams { get; } = new();
    
    /// <summary>
    /// How many server-side uploads can happen at the same time.
    /// </summary>
    public int ParallelUploads { get; set; } = 1;
    
    /// <summary>
    /// This property will be used only if Multiple is true.
    /// </summary>
    public int MaxFiles { get; set; } = int.MaxValue;
    private IHttpContext CurrentContext { get; }
    private UploadAreaService UploadAreaService { get; }
    private MasterDataUrlHelper UrlHelper { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }
    private IEncryptionService EncryptionService { get; }
    
    private RouteContext? _routeContext;

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
    
    public JJUploadArea(
        IHttpContext currentContext,
        UploadAreaService uploadAreaService,
        MasterDataUrlHelper urlHelper,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
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
        if (UploadAreaService.TryGetFile(Multiple ? "uploadAreaFile[0]" : "uploadAreaFile", out var formFile))
            return await GetFileUploadResultAsync(formFile!);
        
        return new RenderedComponentResult(GetUploadAreaHtmlBuilder());
    }

    public async Task<ComponentResult> GetFileUploadResultAsync(FormFileContent formFile)
    {
        if (OnFileUploaded != null)
            UploadAreaService.OnFileUploaded += OnFileUploaded;

        if (OnFileUploadedAsync != null)
            UploadAreaService.OnFileUploadedAsync += OnFileUploadedAsync;

        var dto = await UploadAreaService.UploadFileAsync(formFile, AllowedTypes);
        
        var result = new JsonComponentResult(dto)
        {
            StatusCode = !string.IsNullOrEmpty(dto.ErrorMessage) ? 400 : 200
        };
        
        return result;
    }

    internal HtmlBuilder GetUploadAreaHtmlBuilder()
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", $"{Name}-upload-area-div")
            .WithCssClass("upload-area-div")
            .Append(HtmlTag.Div,  div =>
            {
                div.WithAttributes(Attributes);
                div.WithCssClass("dropzone");
                div.WithAttribute("id", Name);
            });
        div.WithAttributes(Attributes);
        div.WithAttributeIf(Url is not null,"upload-url", Url!);
        div.WithAttribute("js-callback",JsCallback);
        div.WithAttribute((string)"route-context", (string)EncryptionService.EncryptRouteContext(RouteContext));
        div.WithAttribute("allow-multiple-files", Multiple.ToString().ToLower());
        div.WithAttribute("query-string-params", GetQueryStringParams());
        div.WithAttribute("max-file-size", MaxFileSize.ToString());
        div.WithAttribute("allow-drag-drop", EnableDragDrop.ToString().ToLower());
        div.WithAttribute("allow-copy-paste", EnableCopyPaste.ToString().ToLower());
        div.WithAttribute("show-file-size", ShowFileSize.ToString().ToLower());
        div.WithAttribute("allowed-types", GetAllowedTypes());     
        div.WithAttribute("max-files", Multiple ? MaxFiles : 1);
        div.WithAttribute("parallel-uploads", ParallelUploads);
        div.WithAttribute("drag-drop-label", StringLocalizer[GetImportLabel()]);
        div.WithAttribute("cancel-label", StringLocalizer[CancelLabel]);
        div.WithAttribute("abort-label", StringLocalizer[AbortLabel]);
        div.WithAttribute("extension-not-allowed-label", StringLocalizer[ExtensionNotAllowedLabel, AllowedTypes]);
        div.WithAttribute("file-size-error-label", StringLocalizer[SizeErrorLabel, MaxFileSize]);
        
        return div;
    }

    private string GetAllowedTypes()
    {
        if (string.IsNullOrEmpty(AllowedTypes) || AllowedTypes == "*")
            return string.Empty;

        var allowedTypes = new StringBuilder();
        
        foreach (var type in AllowedTypes.Split(','))
        {
            if(allowedTypes.Length > 0)
                allowedTypes.Append(',');
            
            if (type.Trim().StartsWith("."))
                allowedTypes.Append(type);

            else
                allowedTypes.Append("." + type);
        }

        return allowedTypes.ToString();
    }

    /// <summary>
    /// URL where the files are uploaded. If none is provided, they will be sended to the same page with a RouteContext.
    /// </summary>
    public string? Url { get; set; }

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
