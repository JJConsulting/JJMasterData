using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

public class NavFactory(IFormValues formValues) : IComponentFactory<JJTabNav>
{
    public JJTabNav Create()
    {
        return new JJTabNav(formValues);
    }
}