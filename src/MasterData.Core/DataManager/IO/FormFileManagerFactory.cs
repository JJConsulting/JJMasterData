using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.IO;

public class FormFileManagerFactory(IHttpContext httpContext, IStringLocalizer<MasterDataResources> stringLocalizer, ILoggerFactory loggerFactory)
{

    public FormFileManager Create(string memoryFilesSessionName)
    {
        return new FormFileManager(memoryFilesSessionName, httpContext, stringLocalizer,
            loggerFactory.CreateLogger<FormFileManager>());
    }
}