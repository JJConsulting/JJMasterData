using System.Threading.Tasks;
using JJConsulting.Html.Bootstrap.Abstractions;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.UI.Components;

public interface IFormElementComponentFactory<TComponent>  where TComponent : ComponentBase
{
    TComponent Create(FormElement formElement);

    ValueTask<TComponent> CreateAsync(string elementName);
}