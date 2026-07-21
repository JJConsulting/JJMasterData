using JJMasterData.Core.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Web.Components;
using JJMasterData.Web.Extensions;

namespace JJMasterData.Web.Services;

internal sealed class ComponentFileLinkProvider(
    FileDownloaderFactory fileDownloaderFactory,
    IHttpContextAccessor httpContextAccessor) : IExportFileLinkProvider, IFileUrlProvider
{
    private readonly string? _componentUri = httpContextAccessor.HttpContext?.Request.GetAbsoluteUri();

    public string? GetLink(FormElement formElement, FormElementField field,
        IReadOnlyDictionary<string, object?> values, string fileName)
    {
        if (string.IsNullOrEmpty(_componentUri))
            return null;

        var componentValues = values.ToDictionary(item => item.Key, item => item.Value);
        var downloader = fileDownloaderFactory.Create(formElement, field, componentValues, fileName);
        return new Uri(new Uri(_componentUri), downloader.GetDownloadUrl(_componentUri)).AbsoluteUri;
    }

    public string? GetFileUrl(FormElement formElement, FormElementField field,
        IReadOnlyDictionary<string, object?> values, string fileName) =>
        GetLink(formElement, field, values, fileName);
}
