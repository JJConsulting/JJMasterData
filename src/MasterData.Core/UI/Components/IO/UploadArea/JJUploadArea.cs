#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Events.Args;

using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class JJUploadArea : AsyncComponent
{
    private readonly IRequestLengthService _requestLengthService;
    private readonly IHttpContext _httpContext ;
    private readonly UploadAreaService _uploadAreaService ;
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer ;
    private readonly IEncryptionService _encryptionService;
    
    
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

    public string? CustomUploadAreaLabel { get; set; }

    private string GetUploadAreaLabel()
    {
        if (CustomUploadAreaLabel != null)
            return CustomUploadAreaLabel;

        return EnableDragDrop switch
        {
            true when !EnableCopyPaste => "Click here to upload or drag & drop files",
            false when !EnableCopyPaste => "Click here to upload files",
            false when EnableCopyPaste => "Click here to upload or paste files",
            _ => "Click here, paste or drag & drop files"
        };
    }

    public bool EnableDragDrop { get; set; } = true;

    public bool EnableCopyPaste { get; set; } = true;
    public bool ShowFileSize { get; set; } = true;
    public long MaxFileSize { get; set; }

    /// <summary>
    /// JS code to be executed after all server side uploads are completed.
    /// </summary>
    public string JsCallback { get; set; } = "getMasterDataForm().submit()";

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

    
    private RouteContext? _routeContext;

    internal RouteContext RouteContext
    {
        get
        {
            if (_routeContext != null)
                return _routeContext;

            var factory = new RouteContextFactory(_httpContext.Request.QueryString, _encryptionService);
            _routeContext = factory.Create();
            
            return _routeContext;
        }
    }
    
    public JJUploadArea(
        IHttpContext httpContext,
        UploadAreaService uploadAreaService,
        IEncryptionService encryptionService,
        IRequestLengthService requestLengthService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        _requestLengthService = requestLengthService;
        Name = "upload-area";
        _httpContext = httpContext;
        _uploadAreaService = uploadAreaService;
        _stringLocalizer = stringLocalizer;
        _encryptionService = encryptionService;
        MaxFileSize = _requestLengthService.GetMaxRequestBodySize();
    }
    
    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (_uploadAreaService.TryGetFile(Multiple ? "uploadAreaFile[0]" : "uploadAreaFile", out var formFile))
            return await GetFileUploadResultAsync(formFile!);
        
        return new RenderedComponentResult(GetUploadAreaHtmlBuilder());
    }

    public async Task<ComponentResult> GetFileUploadResultAsync(FormFileContent formFile)
    {
        if (OnFileUploaded != null)
            _uploadAreaService.OnFileUploaded += OnFileUploaded;

        if (OnFileUploadedAsync != null)
            _uploadAreaService.OnFileUploadedAsync += OnFileUploadedAsync;

        var dto = await _uploadAreaService.UploadFileAsync(formFile, AllowedTypes);

        var result = new JsonComponentResult(dto);
        
        return result;
    }

    internal HtmlBuilder GetUploadAreaHtmlBuilder()
    {
        if (!Visible)
            return new HtmlBuilder();
        
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
        div.WithAttribute((string)"route-context", _encryptionService.EncryptObject(RouteContext));
        div.WithAttribute("allow-multiple-files", Multiple.ToString().ToLower());
        div.WithAttribute("query-string-params", GetQueryStringParams());
        div.WithAttribute("max-file-size", MaxFileSize.ToString());
        div.WithAttribute("allow-drag-drop", EnableDragDrop.ToString().ToLower());
        div.WithAttribute("allow-copy-paste", EnableCopyPaste.ToString().ToLower());
        div.WithAttribute("show-file-size", ShowFileSize.ToString().ToLower());
        div.WithAttribute("allowed-types", GetAllowedTypes());     
        div.WithAttribute("max-files", Multiple ? MaxFiles.ToString() : 1.ToString());
        div.WithAttribute("parallel-uploads", ParallelUploads.ToString());
        div.WithAttribute("drag-drop-label", _stringLocalizer[GetUploadAreaLabel()]);
        div.WithAttribute("cancel-label", _stringLocalizer[CancelLabel]);
        div.WithAttribute("abort-label", _stringLocalizer[AbortLabel]);
        div.WithAttribute("extension-not-allowed-label", _stringLocalizer[ExtensionNotAllowedLabel, AllowedTypes]);
        div.WithAttribute("file-size-error-label", _stringLocalizer[SizeErrorLabel, MaxFileSize]);
        
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

    public string? ParentName { get; set; }
    
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

    public long GetMaxRequestLength()
    {
        return _requestLengthService.GetMaxRequestBodySize();
    }
}
