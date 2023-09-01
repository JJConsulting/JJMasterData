#nullable enable
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.UI.Components.Controls;

public interface IDynamicControlFactory<out TControl> : IControlFactory<TControl> where TControl : ControlBase
{
    public TControl Create(
        FormElement formElement, 
        IDictionary<string, object?> values,
        IDictionary<string, object?> userValues);
}