using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Hosting;
internal class PostConfigureStaticFileOptions : IPostConfigureOptions<StaticFileOptions>
{
    private readonly IWebHostEnvironment _environment;
    
    public PostConfigureStaticFileOptions(IWebHostEnvironment environment)
    {
        _environment = environment;
    }
    public void PostConfigure(string name, StaticFileOptions options)
    {
        if (options.FileProvider == null && _environment.WebRootFileProvider == null)
        {
            throw new InvalidOperationException("Missing FileProvider.");
        }
        
        options.FileProvider = _environment.WebRootFileProvider;

        var filesProvider = new ManifestEmbeddedFileProvider(GetType().Assembly, "wwwroot");
        options.FileProvider = new CompositeFileProvider(options.FileProvider, filesProvider);
    }
}