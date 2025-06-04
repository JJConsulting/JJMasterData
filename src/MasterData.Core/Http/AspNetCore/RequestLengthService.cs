#if NET
using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.Http.AspNetCore;

internal sealed class RequestLengthService(IOptions<FormOptions> options) : IRequestLengthService
{
    private readonly long? _maxRequestBodySize = options.Value.MultipartBodyLengthLimit;

    public long GetMaxRequestBodySize()
    {
        return _maxRequestBodySize ?? 30720000;
    }
}
#endif