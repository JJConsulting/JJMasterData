#if NETFRAMEWORK
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Http.SystemWeb;

internal sealed class SystemWebRequestLengthService : IRequestLengthService
{
    private readonly long? _maxRequestBodySize;

    public SystemWebRequestLengthService()
    {
        var maxRequestLengthInKb = GetMaxRequestLengthFromWebConfig();
        _maxRequestBodySize = maxRequestLengthInKb * 1024L;
    }

    public long GetMaxRequestBodySize()
    {
        return _maxRequestBodySize ?? 4194304;
    }

    private static int? GetMaxRequestLengthFromWebConfig()
    {
        var maxRequestLength = 4194304; //4mb
        if (System.Configuration.ConfigurationManager.GetSection("system.web/httpRuntime") is System.Web.Configuration.HttpRuntimeSection section)
            maxRequestLength = section.MaxRequestLength * 1024;
        
        return maxRequestLength;
    }
}
#endif  