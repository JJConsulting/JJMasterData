using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JJMasterData.Web.Example.Pages;

public class IndexModel : PageModel
{
    public ActionResult OnGetDownloadFile()
    {
        var file = System.IO.File.Open("Product.json", FileMode.Open) as Stream;
        return File(file, "application/octet-stream", "Product.json");
    }
}