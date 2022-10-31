#nullable disable

using JJMasterData.Core.WebComponents;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JJMasterData.Example.Pages;

public class TitleExample : PageModel
{
    public JJTitle Title;

    public TitleExample()
    {
        Title = new JJTitle("JJTitle"," the most simple component.");
    }
}