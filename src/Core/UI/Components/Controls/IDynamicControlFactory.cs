#nullable enable
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components.Abstractions;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.UI.Components.Controls;

public interface IDynamicControlFactory<out TControl> : IControlFactory<TControl> where TControl : AsyncControl
{
    public TControl? CreateIfExists(
        FormElement formElement, 
        IDictionary<string, object?> values,
        IDictionary<string, object?> userValues);
}