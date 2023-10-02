using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

public class NavFactory : IComponentFactory<JJTabNav>
{
    private readonly IFormValues _formValues;

    public NavFactory(IFormValues formValues)
    {
        _formValues = formValues;
    }
    
    public JJTabNav Create()
    {
        return new JJTabNav(_formValues);
    }
}