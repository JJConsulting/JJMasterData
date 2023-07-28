using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.UI.Components;

public interface IFormElementComponentFactory : IComponentFactory
{
    
}

public interface IFormElementComponentFactory<TComponent> : IFormElementComponentFactory where TComponent : JJBaseView
{
    TComponent Create(FormElement formElement);

    Task<TComponent> CreateAsync(string elementName);
}