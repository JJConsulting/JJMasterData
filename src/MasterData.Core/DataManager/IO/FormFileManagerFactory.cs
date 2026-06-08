using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using JJMasterData.Core.DataManager.IO.Storage;
using Microsoft.Extensions.Primitives;

namespace JJMasterData.Core.DataManager.IO;

public class FormFileManagerFactory(
    IHttpContextAccessor httpContext,
    ITemporaryUploadStore temporaryUploadStore,
    IFileStorage fileStorage,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ILoggerFactory loggerFactory)
{

    public FormFileManager Create(string uploadName)
    {
        var request = httpContext.HttpContext?.Request;
        var draftId = GetFormValue(request, $"{uploadName}-draft-id");
        if (string.IsNullOrEmpty(draftId))
            draftId = request == null ? null : GetFirstValue(request.Query["draftId"]);

        return CreateWithDraft(draftId);
    }

    public FormFileManager CreateWithDraft(string draftId)
    {
        return new FormFileManager(draftId, temporaryUploadStore, fileStorage, stringLocalizer,
            loggerFactory.CreateLogger<FormFileManager>());
    }

    private static string GetFirstValue(StringValues values)
    {
        return values.Count == 0 ? null : values[0];
    }

    private static string GetFormValue(HttpRequest request, string key)
    {
        if (request?.HasFormContentType != true)
            return null;

        return GetFirstValue(request.Form[key]);
    }
}
