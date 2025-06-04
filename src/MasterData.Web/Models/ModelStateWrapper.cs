﻿using JJMasterData.Core.DataDictionary.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JJMasterData.Web.Models;

internal sealed class ModelStateWrapper(IActionContextAccessor actionContextAccessor) : IValidationDictionary
{
    private ModelStateDictionary ModelState =>
        actionContextAccessor.ActionContext?.ModelState ?? new ModelStateDictionary();
    public IEnumerable<string> Errors => 
        ModelState.Values.SelectMany(entry => entry.Errors.Select(e => e.ErrorMessage));

    public void AddError(string key, string errorMessage) => ModelState.AddModelError(key, errorMessage);
    public void RemoveError(string key) => ModelState.Remove(key);
    public bool IsValid => ModelState.IsValid;
}