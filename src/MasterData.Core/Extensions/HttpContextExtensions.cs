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

        /// <summary>
        /// Recover the value of a key from the query string or form data. Query string takes precedence over form data.
        /// </summary>
        public string? GetValue(string key)
        {
            if (request.Query.ContainsKey(key))
                return request.Query[key];

            if (request.HasFormContentType)
                return request.Form[key];

            return null;
        }
        
        public string? GetFormValue(string key)
        {
            return request.HasFormContentType ? request.Form[key] : (string?)null;
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

    extension(IOptions<FormOptions> options)
    {
        public long GetMaxRequestBodySize()
        {
            return options.Value.MultipartBodyLengthLimit;
        }
    }
}
