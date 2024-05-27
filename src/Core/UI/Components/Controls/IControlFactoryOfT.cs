using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Factory class used to create Controls.
/// </summary>
/// <typeparam name="TControl"></typeparam>
public interface IControlFactory<out TControl> where TControl : ControlBase
{
    public TControl Create();
    internal TControl Create(
        FormElement formElement,
        FormElementField field,
        ControlContext context);
}