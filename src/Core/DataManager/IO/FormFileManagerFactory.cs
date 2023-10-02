using JJMasterData.Commons.Localization;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.IO;

public class FormFileManagerFactory
{
    private IHttpContext HttpContext { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ILoggerFactory LoggerFactory { get; }

    public FormFileManagerFactory(IHttpContext httpContext, IStringLocalizer<JJMasterDataResources> stringLocalizer, ILoggerFactory loggerFactory)
    {
        HttpContext = httpContext;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
    }

    public FormFileManager Create(string memoryFilesSessionName)
    {
        return new FormFileManager(memoryFilesSessionName, HttpContext, StringLocalizer,
            LoggerFactory.CreateLogger<FormFileManager>());
    }
}