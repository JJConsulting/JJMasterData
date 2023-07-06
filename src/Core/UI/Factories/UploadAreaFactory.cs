using JJMasterData.Commons.Localization;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Factories;

public class UploadAreaFactory
{
    private IHttpContext HttpContext { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public UploadAreaFactory(IHttpContext httpContext, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        HttpContext = httpContext;
        StringLocalizer = stringLocalizer;
    }

    public JJUploadArea CreateUploadArea()
    {
        return new JJUploadArea(HttpContext, StringLocalizer);
    }
}   