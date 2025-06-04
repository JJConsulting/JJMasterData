using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.UI.Components;


public interface IComponentFactory<out TComponent> where TComponent : ComponentBase
{
    TComponent Create();
}

public interface IFormElementComponentFactory<TComponent>  where TComponent : ComponentBase
{
    TComponent Create(FormElement formElement);

    ValueTask<TComponent> CreateAsync(string elementName);
}