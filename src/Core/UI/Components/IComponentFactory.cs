using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using System.Threading.Tasks;

namespace JJMasterData.Core.UI.Components;

public interface IComponentFactory
{
    
}

public interface IComponentFactory<out TComponent> : IComponentFactory where TComponent : ComponentBase
{
    TComponent Create();
}

public interface IFormElementComponentFactory<TComponent> : IComponentFactory where TComponent : ComponentBase
{
    TComponent Create(FormElement formElement);

    Task<TComponent> CreateAsync(string elementName);
}