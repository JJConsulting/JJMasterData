using JJMasterData.Commons.Localization;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.IO;

public class FormFileManagerFactory(IHttpContext httpContext, IStringLocalizer<MasterDataResources> stringLocalizer, ILoggerFactory loggerFactory)
{
    private IHttpContext HttpContext { get; } = httpContext;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private ILoggerFactory LoggerFactory { get; } = loggerFactory;

    public FormFileManager Create(string memoryFilesSessionName)
    {
        return new FormFileManager(memoryFilesSessionName, HttpContext, StringLocalizer,
            LoggerFactory.CreateLogger<FormFileManager>());
    }
}