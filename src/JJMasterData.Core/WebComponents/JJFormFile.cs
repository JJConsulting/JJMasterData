#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace JJMasterData.Core.WebComponents;

public class JJFormFile
{

#if NETFRAMEWORK
    public HttpPostedFile FileData { get; set; }
    public JJFormFile(HttpPostedFile file)
    {
        FileData = file;
    }
#else
    public JJFormFile(IFormFile file)
    {
        FileData = file;
    }
    public IFormFile FileData { get; set; }
#endif
}
