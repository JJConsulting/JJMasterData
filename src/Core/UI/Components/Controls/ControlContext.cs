using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.Web.Factories;

public record ControlContext(
    FormElement FormElement,
    FormElementField Field,
    FormStateData FormStateData,
    string ParentName,
    object Value);