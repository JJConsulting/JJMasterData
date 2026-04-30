#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Services.Abstractions;

public interface IValidationScriptExecutor
{
    RuleLanguage Language { get; }

    Task<Dictionary<string, string>> ExecuteAsync(
        FormElement formElement,
        FormElementRule rule,
        Dictionary<string, object?> values);
}
