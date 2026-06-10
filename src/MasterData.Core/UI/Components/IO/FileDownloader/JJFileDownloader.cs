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
    IHttpContextAccessor httpContextAccessor,
    IFileStorage fileStorage,
    IEncryptionService encryptionService,
    IStringLocalizer<MasterDataResources> stringLocalizer) : HtmlComponent
{
    private const string FileTokenParameter = "downloadFileToken";

    public FileStorageItemKey File { get; set; }

    protected override HtmlBuilder BuildHtml()
    {
        if (File == null)
            throw new JJMasterDataException(stringLocalizer["Invalid file reference"]);

        return new HtmlBuilder();
    }

    public async Task<FileStreamComponentResult> GetDirectDownloadResultAsync()
    {
        if (File == null)
            throw new ArgumentNullException(nameof(File));

        var stream = await fileStorage.OpenReadAsync(File.FullPath);
        return new FileStreamComponentResult(stream, File.FileName);
    }

    public async Task<FileStreamComponentResult> GetDownloadResultAsync()
    {
        var token = httpContextAccessor.HttpContext!.Request.Query[FileTokenParameter].ToString();
        if (string.IsNullOrEmpty(token))
            throw new JJMasterDataException("Invalid file token.");

        var fileKey = encryptionService.DecryptObject<FileStorageItemKey>(token);
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

        query["routeContext"] = encryptionService.EncryptObject(routeContext);
        query[FileTokenParameter] = encryptionService.EncryptObject(File);

        uriBuilder.Query = query.ToString()!;

        return uriBuilder.Uri.PathAndQuery;
    }

    public string GetDownloadUrl()
    {
        return GetDownloadUrl(httpContextAccessor.HttpContext!.Request.GetAbsoluteUri());
    }
}