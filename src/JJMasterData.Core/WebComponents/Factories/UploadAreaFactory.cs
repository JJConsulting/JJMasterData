using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.WebComponents.Factories;

public class UploadAreaFactory
{
    private readonly IHttpContext _httpContext;

    public UploadAreaFactory(IHttpContext httpContext)
    {
        _httpContext = httpContext;
    }

    public JJUploadArea CreateUploadArea()
    {
        return new JJUploadArea(_httpContext);
    }
}