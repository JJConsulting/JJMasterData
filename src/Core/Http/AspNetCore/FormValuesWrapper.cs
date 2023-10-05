#if NET

#nullable enable

using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.Http.AspNetCore;

internal class FormValuesWrapper : IFormValues
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

    private HttpContext HttpContext { get; }
    
    public bool ContainsFormValues() => FormCollection is not null;

    public string? this[string key] => FormCollection?[key];

    public FormValuesWrapper(IHttpContextAccessor httpContextAccessor)
    {
        HttpContext = httpContextAccessor.HttpContext;
    }
    public IFormFile? GetFile(string file) => FormCollection?.Files[file];
}
#endif