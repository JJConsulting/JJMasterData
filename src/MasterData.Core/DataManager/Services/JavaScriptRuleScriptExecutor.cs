#nullable enable

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class JavaScriptRuleScriptExecutor : IValidationScriptExecutor
{
    public RuleLanguage Language => RuleLanguage.JavaScript;

    public async Task<Dictionary<string, string>> ExecuteAsync(
        FormElement formElement,
        FormElementRule rule,
        Dictionary<string, object?> values)
    {
        var errors = new Dictionary<string, string>();
        var index = 0;
        IDictionary<string, object?> valuesObject = new ExpandoObject();
        foreach (var value in values)
        {
            valuesObject[value.Key] = value.Value;
        }

        using var engine = new Engine();
        engine.SetValue("values", valuesObject);
        engine.SetValue("addError", new Action<JsValue, JsValue?>((first, second) =>
        {
            var hasSecondArgument = !second?.IsUndefined() ?? false;
            var key = hasSecondArgument ? first.ToString() : (++index).ToString();
            var message = hasSecondArgument ? second?.ToString() : first.ToString();

            if (string.IsNullOrWhiteSpace(message))
                return;

            if (string.IsNullOrWhiteSpace(key))
                key = (++index).ToString();

            if (!errors.ContainsKey(key))
                errors[key] = message ?? string.Empty;
        }));
       
        await engine.ExecuteAsync(rule.Script);
        
        return errors;
    }
}
