using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Factory class used to create Controls.
/// </summary>
/// <typeparam name="TControl"></typeparam>
public interface IControlFactory<out TControl> where TControl : ControlBase
{
    public TControl Create();
    internal TControl Create(FormElement formElement,
        FormElementField field,
        ControlContext context);
}