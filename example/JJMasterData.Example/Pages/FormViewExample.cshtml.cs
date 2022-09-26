#nullable disable
using JJMasterData.Core.WebComponents;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JJMasterData.Example.Pages;
public class FormViewExample : PageModel
{
    public JJFormView FormView;

    public FormViewExample()
    {
        FormView = new("Product");
        FormView.FormElement.SubTitle = "Represents a full CRUD form.";
    }
}