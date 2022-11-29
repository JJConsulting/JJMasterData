#nullable disable
using JJMasterData.Core.WebComponents;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JJMasterData.Web.Example.Pages;
public class JJFormViewExample : PageModel
{
    public JJFormView FormView;

    public JJFormViewExample()
    {
        FormView = new JJFormView("Product")
        {
            FormElement =
            {
                SubTitle = "Represents a full CRUD form."
            }
        };
    }
    
    public void OnGet()
    {
        
    }
}