#if NET
using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace JJMasterData.Core.Http.AspNetCore;

    internal class HttpRequestWrapper(
        IHttpContextAccessor httpContextAccessor,
        IFormValues formValues,
        IQueryString queryString)
        : IHttpRequest
    {
        public IQueryString QueryString { get; } = queryString;
        public IFormValues Form { get; } = formValues;

        private HttpRequest Request => httpContextAccessor.HttpContext?.Request;

        private HttpContext HttpContext => httpContextAccessor.HttpContext;

        public IFormFile GetFile(string file)
        {
            return Request?.HasFormContentType == true ? Request.Form.Files[file] : null;
        }

        public string this[string key] => GetValue(key);

        public string UserHostAddress => HttpContext?.Connection.RemoteIpAddress?.ToString();

        public string HttpMethod => Request?.Method;

        public string UserAgent => Request?.Headers.UserAgent;

        public string AbsoluteUri => Request?.GetDisplayUrl();

        public string ApplicationPath => Request?.PathBase;

        public bool IsPost => HttpMethod?.Equals("POST") == true;

        public string ContentType => Request?.ContentType;
        
        private string GetValue(string key)
        {
            if (Request?.Query.ContainsKey(key) == true)
                return Request.Query[key];

            if (Request?.HasFormContentType == true)
                return Request.Form[key];

            return null;
        }
    }

#endif  