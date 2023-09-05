#nullable enable
using System.Threading.Tasks;

using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;

namespace JJMasterData.Core.Web;

public interface IControlFactory
{
    IControlFactory<TControl> GetControlFactory<TControl>() where TControl : ControlBase;
    TControl Create<TControl>() where TControl : ControlBase;
    internal TControl Create<TControl>(FormElement formElement,FormElementField field, ControlContext controlContext) where TControl : ControlBase;

    internal ControlBase Create(
        FormElement formElement,
        FormElementField field,
        ControlContext context);

    internal Task<ControlBase> CreateAsync(
        FormElement formElement,
        FormElementField field,
        FormStateData formStateData,
        object? value = null);
}