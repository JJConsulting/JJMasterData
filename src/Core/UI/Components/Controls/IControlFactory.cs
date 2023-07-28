using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;

namespace JJMasterData.Core.Web;

/// <summary>
/// Marker class used to identify control factories.
/// </summary>
public interface IControlFactory : IComponentFactory
{

}


/// <summary>
/// Factory class used to create instance of JJBaseControls.
/// </summary>
/// <typeparam name="TControl"></typeparam>
public interface IControlFactory<out TControl> : IControlFactory where TControl : JJBaseControl
{
    public TControl Create();
    public TControl Create(
        FormElement formElement,
        FormElementField field, 
        ControlContext context);
}