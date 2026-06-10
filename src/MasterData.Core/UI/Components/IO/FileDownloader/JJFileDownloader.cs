using System;
using System.Threading.Tasks;
using System.Web;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataManager.Storage;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class JJFileDownloader(
        IHttpContextAccessor currentContext,
        IFileStorage fileStorage,
        ITemporaryFileStore temporaryFileStore,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : HtmlComponent
{
    public const string FileTokenParameter = "downloadFileToken";

    public FileStorageItemKey File { get; set; }

    internal IHttpContextAccessor CurrentContext { get; } = currentContext;
    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    internal IEncryptionService EncryptionService { get; } = encryptionService;

    protected override HtmlBuilder BuildHtml()
    {
        if (File == null)
            throw new JJMasterDataException(StringLocalizer["Invalid file reference"]);

        return new HtmlBuilder();
    }

    public async Task<FileStreamComponentResult> GetDirectDownloadResultAsync()
    {
        if (File == null)
            throw new ArgumentNullException(nameof(File));

        var storage = File.IsTemporary ? temporaryFileStore : fileStorage;
        var stream = await storage.OpenReadAsync(File.FolderPath, File.FileName);
        return new FileStreamComponentResult(stream, File.FileName);
    }
    
    public async Task<FileStreamComponentResult> GetDownloadResultAsync()
    {
        var token = CurrentContext.HttpContext!.Request.Query[FileTokenParameter].ToString();
        if (string.IsNullOrEmpty(token))
            throw new JJMasterDataException("Invalid file token.");

        var fileKey = EncryptionService.DecryptObject<FileStorageItemKey>(token);
        File = fileKey;
        
        return await GetDirectDownloadResultAsync();
    }
    
    public string GetDownloadUrl(string absoluteUri)
    {
        if (File == null)
            throw new ArgumentNullException(nameof(File));

        var uriBuilder = new UriBuilder(absoluteUri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        var routeContext = new RouteContext(ComponentContext.DownloadFile);

        query["routeContext"] = EncryptionService.EncryptObject(routeContext);
        query[FileTokenParameter] = EncryptionService.EncryptObject(File);

        uriBuilder.Query = query.ToString()!;

        return uriBuilder.Uri.PathAndQuery;
    }

    public string GetDownloadUrl()
    {
        return GetDownloadUrl(CurrentContext.HttpContext!.Request.GetAbsoluteUri());
    }
}
