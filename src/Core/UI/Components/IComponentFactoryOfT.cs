using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using System.Threading.Tasks;

namespace JJMasterData.Core.UI.Components;


public interface IComponentFactory<out TComponent> where TComponent : ComponentBase
{
    TComponent Create();
}

public interface IFormElementComponentFactory<TComponent>  where TComponent : ComponentBase
{
    TComponent Create(FormElement formElement);

    Task<TComponent> CreateAsync(string elementName);
}