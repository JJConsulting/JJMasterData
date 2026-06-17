using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class JJFileDownloader(
    IHttpContextAccessor httpContextAccessor,
    IFileStorage fileStorage,
    IEncryptionService encryptionService) 
{
    private const string FileTokenParameter = "downloadFileToken";

    public string FullPath { get; set; }
    
    public async Task<FileStreamComponentResult> GetDirectDownloadResultAsync()
    {
        if (string.IsNullOrEmpty(FullPath))
            throw new ArgumentNullException(nameof(FullPath));

        var stream = await fileStorage.OpenReadAsync(FullPath);
        
        var fileName = Path.GetFileName(FullPath);
        
        return new FileStreamComponentResult(stream, fileName);
    }

    public async Task<FileStreamComponentResult> GetDownloadResultAsync()
    {
        var token = httpContextAccessor.HttpContext!.Request.Query[FileTokenParameter].ToString();
        if (string.IsNullOrEmpty(token))
            throw new JJMasterDataException("Invalid file token.");
        
        FullPath = encryptionService.DecryptStringWithUrlUnescape(token);

        return await GetDirectDownloadResultAsync();
    }

    public string GetDownloadUrl(string absoluteUri)
    {
        if (FullPath == null)
            throw new ArgumentNullException(nameof(FullPath));

        var uriBuilder = new UriBuilder(absoluteUri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        var routeContext = new RouteContext(ComponentContext.DownloadFile);

        query["routeContext"] = encryptionService.EncryptObject(routeContext);
        query[FileTokenParameter] = encryptionService.EncryptStringWithUrlEscape(FullPath);

        uriBuilder.Query = query.ToString()!;

        return uriBuilder.Uri.PathAndQuery;
    }

    public string GetDownloadUrl()
    {
        return GetDownloadUrl(httpContextAccessor.HttpContext!.Request.GetAbsoluteUri());
    }
}