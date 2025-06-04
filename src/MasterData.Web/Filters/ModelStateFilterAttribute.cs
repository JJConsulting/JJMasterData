using System.Text.Json;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JJMasterData.Web.Filters;

public abstract class ModelStateFilterAttribute : ActionFilterAttribute
{
    protected const string Key = "ModelStateErrors";
    
    protected static string SerialiseModelState(ModelStateDictionary modelState)
    {
        var errorList = modelState
            .Select(kvp => new ModelStateTransferValue
            {
                Key = kvp.Key,
                AttemptedValue = kvp.Value?.AttemptedValue,
                RawValue = kvp.Value?.RawValue,
                ErrorMessages = kvp.Value?.Errors.Select(err => err.ErrorMessage).ToList() ?? [],
            });

        return JsonSerializer.Serialize(errorList);
    }

    protected static ModelStateDictionary DeserializeModelState(string serialisedErrorList)
    {
        var errorList = JsonSerializer.Deserialize<List<ModelStateTransferValue>>(serialisedErrorList);
        var modelState = new ModelStateDictionary();

        if (errorList != null)
            foreach (var item in errorList)
            {
                modelState.SetModelValue(item.Key, item.RawValue, item.AttemptedValue);
                foreach (var error in item.ErrorMessages)
                {
                    modelState.AddModelError(item.Key, error);
                }
            }

        return modelState;
    }
}