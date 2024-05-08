using JJMasterData.Core.DataDictionary.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JJMasterData.Web.Models;

public class ModelStateWrapper(IActionContextAccessor actionContextAccessor) : IValidationDictionary
{
    private readonly ModelStateDictionary _modelState = actionContextAccessor.ActionContext?.ModelState ?? new ModelStateDictionary();

    public IEnumerable<string> Errors => 
        _modelState.Values.SelectMany(entry => entry.Errors.Select(e => e.ErrorMessage));

    public void AddError(string key, string errorMessage) => _modelState.AddModelError(key, errorMessage);
    public void RemoveError(string key) => _modelState.Remove(key);
    public bool IsValid => _modelState.IsValid;
}