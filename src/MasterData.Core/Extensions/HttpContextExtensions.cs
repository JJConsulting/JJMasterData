#nullable enable
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.Extensions;

public static class HttpContextExtensions
{
    extension(HttpRequest request)
    {
        public string GetApplicationPath()
        {
            return request.PathBase.ToString();
        }

        public string? GetFormValue(string key)
        {
            return request.HasFormContentType ? request.Form[key].ToString() : null;
        }
        
        public string GetApplicationUri()
        {
            return new Uri($"{request.Scheme}://{request.Host}{request.PathBase}").ToString();
        }

        public string GetAbsoluteUri()
        {
            return request.GetDisplayUrl();
        }
    }

    public static long GetMaxRequestBodySize(this IOptions<FormOptions> options)
    {
        return options.Value.MultipartBodyLengthLimit;
    }
}
