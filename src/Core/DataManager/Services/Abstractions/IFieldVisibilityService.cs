using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public interface IFieldVisibilityService
{
    Task<bool> IsVisibleAsync(FormElementField field, PageState state, IDictionary<string,dynamic>formValues);
    Task<bool> IsEnabledAsync(FormElementField field, PageState state, IDictionary<string,dynamic>formValues);
}