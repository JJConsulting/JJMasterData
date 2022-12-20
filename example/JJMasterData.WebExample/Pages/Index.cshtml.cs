using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JJMasterData.WebExample.Pages;

public class IndexModel : PageModel
{
    public ActionResult OnGetDownloadFile()
    {
        var file = System.IO.File.Open("Metadata/Example.json", FileMode.Open) as Stream;
        return File(file, "application/octet-stream", "Example.json");
    }
}