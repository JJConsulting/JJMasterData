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
        ITemporaryUploadStore temporaryUploadStore,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : HtmlComponent
{
    public const string FileTokenParameter = "downloadFileToken";

    public FileStorageReference FileReference { get; set; }

    internal IHttpContextAccessor CurrentContext { get; } = currentContext;
    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    internal IEncryptionService EncryptionService { get; } = encryptionService;

    protected override HtmlBuilder BuildHtml()
    {
        if (FileReference == null)
            throw new JJMasterDataException(StringLocalizer["Invalid file reference"]);

        return new HtmlBuilder();
    }

    public async Task<FileStreamComponentResult> GetDirectDownloadResultAsync()
    {
        if (FileReference == null)
            throw new ArgumentNullException(nameof(FileReference));

        var storage = FileReference.IsTemporary ? temporaryUploadStore : fileStorage;
        var stream = await storage.OpenReadAsync(FileReference.FolderPath, FileReference.FileName);
        return new FileStreamComponentResult(stream, FileReference.FileName);
    }

    public async Task<FileStreamComponentResult> GetDownloadResultAsync()
    {
        var token = CurrentContext.HttpContext!.Request.Query[FileTokenParameter].ToString();
        if (string.IsNullOrEmpty(token))
            throw new JJMasterDataException("Invalid file token.");

        var fileToken = EncryptionService.DecryptObject<FileStorageReference>(token);

        FileReference = fileToken;
        
        return await GetDirectDownloadResultAsync();
    }

    public string GetDownloadUrl()
    {
        return GetDownloadUrl(CurrentContext.HttpContext!.Request.GetAbsoluteUri());
    }

    public string GetDownloadUrl(string absoluteUri)
    {
        if (FileReference == null)
            throw new ArgumentNullException(nameof(FileReference));

        var uriBuilder = new UriBuilder(absoluteUri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        var routeContext = new RouteContext(ComponentContext.DownloadFile);

        query["routeContext"] = EncryptionService.EncryptObject(routeContext);
        query[FileTokenParameter] = EncryptionService.EncryptObject(FileReference);

        uriBuilder.Query = query.ToString()!;

        return uriBuilder.Uri.PathAndQuery;
    }
}
