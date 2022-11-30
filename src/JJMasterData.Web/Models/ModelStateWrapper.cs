using JJMasterData.Core.DataDictionary.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JJMasterData.Web.Models;

public class ModelStateWrapper : IValidationDictionary
{
    private readonly ModelStateDictionary _modelState;

    public ModelStateWrapper(IActionContextAccessor actionContextAccessor)
    {
        _modelState = actionContextAccessor.ActionContext!.ModelState;
    }

    public IEnumerable<string> Errors => 
        _modelState.Values.SelectMany(entry => entry.Errors.Select(e => e.ErrorMessage));

    public void AddError(string key, string errorMessage) => _modelState.AddModelError(key, errorMessage);

    public bool IsValid => _modelState.IsValid;
}