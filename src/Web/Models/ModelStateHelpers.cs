using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace JJMasterData.Web.Models;

public static class ModelStateHelpers
{
    public static string SerialiseModelState(ModelStateDictionary modelState)
    {
        var errorList = modelState
            .Select(kvp => new ModelStateTransferValue
            {
                Key = kvp.Key,
                AttemptedValue = kvp.Value?.AttemptedValue,
                RawValue = kvp.Value?.RawValue,
                ErrorMessages = kvp.Value?.Errors.Select(err => err.ErrorMessage).ToList() ?? [],
            });

        return JsonConvert.SerializeObject(errorList);
    }

    public static ModelStateDictionary DeserialiseModelState(string serialisedErrorList)
    {
        var errorList = JsonConvert.DeserializeObject<List<ModelStateTransferValue>>(serialisedErrorList);
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