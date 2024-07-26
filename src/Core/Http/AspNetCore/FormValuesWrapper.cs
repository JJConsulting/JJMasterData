#if NET

#nullable enable

using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.Http.AspNetCore;

internal sealed class FormValuesWrapper(IHttpContextAccessor httpContextAccessor) : IFormValues
{
    private IFormCollection? _formCollection;

    private IFormCollection? FormCollection
    {
        get
        {
            if (_formCollection == null && HttpContext.Request.HasFormContentType)
                _formCollection = HttpContext.Request.Form;
            
            return _formCollection;
        }
    }

    private HttpContext HttpContext { get; } = httpContextAccessor.HttpContext!;

    public bool ContainsFormValues() => FormCollection is not null;

    public string? this[string key] => FormCollection?[key];

    public IFormFile? GetFile(string file) => FormCollection?.Files[file];
}
#endif