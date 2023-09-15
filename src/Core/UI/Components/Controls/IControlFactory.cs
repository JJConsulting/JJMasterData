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
    public IControlFactory<JJCheckBox> CheckBox { get; }
    public IControlFactory<JJComboBox> ComboBox { get; }
    public IControlFactory<JJLookup> Lookup { get; }
    public IControlFactory<JJSearchBox> SearchBox { get; }
    public IControlFactory<JJSlider> Slider { get; }
    public IControlFactory<JJTextArea> TextArea { get; }
    public IControlFactory<JJTextBox> TextBox { get; }
    public IControlFactory<JJTextGroup> TextGroup { get; }
    public IControlFactory<JJTextFile> TextFile { get; }
    public IControlFactory<JJTextRange> TextRange { get; }
    
    internal TControl Create<TControl>(
        FormElement formElement,
        FormElementField field, 
        ControlContext controlContext) where TControl : ControlBase;

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