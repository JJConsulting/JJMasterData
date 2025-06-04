#if NET

#nullable enable

using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.Http.AspNetCore;

internal sealed class FormValuesWrapper(IHttpContextAccessor httpContextAccessor) : IFormValues
{
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext!;

    private IFormCollection? _formCollection;

    private IFormCollection? FormCollection
    {
        get
        {
            if (_formCollection == null && _httpContext.Request.HasFormContentType)
                _formCollection = _httpContext.Request.Form;
            
            return _formCollection;
        }
    }
    
    public bool ContainsFormValues() => FormCollection is not null;

    public string? this[string key] => FormCollection?[key];

    public IFormFile? GetFile(string file) => FormCollection?.Files[file];
}
#endif